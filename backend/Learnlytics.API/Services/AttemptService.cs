using Learnlytics.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Learnlytics.API.Services
{
    public class AttemptService
    {
        private readonly IMongoCollection<Attempt> _attempts;
        private readonly IMongoCollection<Assessment> _assessments;
        private readonly IMongoCollection<PlagiarismReport> _plagiarism;
        private readonly ILogger<AttemptService> _logger; // Add this field

        public AttemptService(IOptions<MongoSettings> settings, ILogger<AttemptService> logger) // Add logger parameter
        {
            _logger = logger; // Assign logger

            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _attempts = database.GetCollection<Attempt>("Attempts");
            _assessments = database.GetCollection<Assessment>("Assessments");
            _plagiarism = database.GetCollection<PlagiarismReport>("PlagiarismReport");

            _attempts.Indexes.CreateMany(new[]
            {
                new CreateIndexModel<Attempt>(
                    Builders<Attempt>.IndexKeys.Ascending(a => a.AssessmentId)),
                new CreateIndexModel<Attempt>(
                    Builders<Attempt>.IndexKeys.Ascending(a => a.Username)),
            });
        }

        public async Task<Attempt> StartAttemptAsync(string assessmentId, string username)
        {
            _logger.LogInformation("Attempting to start assessment. AssessmentId: {AssessmentId}, Username: {Username}",
                                    assessmentId, username);

            var assessment = await _assessments
                .Find(a => a.Id == assessmentId && a.Published)
                .FirstOrDefaultAsync();

            if (assessment == null)
            {
                _logger.LogWarning("Assessment not found. AssessmentId: {AssessmentId}", assessmentId);
                throw new ArgumentException("Assessment not found", nameof(assessmentId));
            }

            _logger.LogInformation("Assessment found. AssessmentId: {AssessmentId}, Published: {Published}",
                                    assessment.AssessmentId, assessment.Published);

            if (!assessment.Published)
            {
                _logger.LogWarning("Assessment is not published. AssessmentId: {AssessmentId}", assessmentId);
                throw new ArgumentException("Assessment not published", nameof(assessmentId));
            }

            var now = DateTime.UtcNow;
            var attempt = new Attempt
            {
                AssessmentId = assessment.AssessmentId,
                Username = username,
                StartedAt = now,
                ExpiredAt = now.AddMinutes(assessment.DurationMinutes),
                Status = AttemptStatus.InProgress
            };

            await _attempts.InsertOneAsync(attempt);

            _logger.LogInformation("Attempt started successfully. AttemptId: {AttemptId}, Username: {Username}",
                                    attempt.Id, username);

            return attempt;
        }

        public async Task<Attempt?> GetAttemptAsync(string attemptId)
        {
            return await _attempts.Find(a => a.Id == attemptId).FirstOrDefaultAsync();
        }

        public async Task SubmitAnswerAsync(string attemptId, List<Answers> answers)
        {
            var attempt = await GetAttemptAsync(attemptId) ?? throw new InvalidOperationException("Attempt not found.");

            if (DateTime.UtcNow > attempt.ExpiredAt)
            {
                attempt.Status = AttemptStatus.Expired;
                await _attempts.ReplaceOneAsync(x => x.Id == attempt.Id, attempt);
                throw new InvalidOperationException("Time is over.");
            }

            attempt.Answers = answers;

            // Auto-score MCQs
            var assessment = await _assessments.Find(a => a.Id == attempt.AssessmentId).FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Assessment missing.");

            int autoScore = 0;
            foreach(var q in assessment.Questions)
            {
                var ans = answers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (q is McqQuestion mcq && ans is McqAnswer mcqa)
                {
                    bool isCorrect = mcqa.SelectedOptions.Count == mcq.CorrectAnswers.Count &&
                                     !mcqa.SelectedOptions.Except(mcq.CorrectAnswers).Any();
                    if (isCorrect) { 
                        autoScore += mcq.Points;
                        mcqa.IsCorrect = true;
                    } else {
                        mcqa.IsCorrect = false;
                    }
                }
            }

            attempt.AutoScore = autoScore;
            attempt.Status = AttemptStatus.Submitted;
            attempt.SubmittedAt = DateTime.UtcNow;

            await _attempts.ReplaceOneAsync(x => x.Id == attempt.Id, attempt);
            await GeneratePlagiarismReportAsync(assessment, attempt);
        }

        private async Task GeneratePlagiarismReportAsync(Assessment assessment, Attempt attempt)
        {
            var codingQs = assessment.Questions.OfType<CodingQuestion>().ToList();
            if (!codingQs.Any()) return;

            foreach (var cq in codingQs)
            {
                var thisAns = attempt.Answers.OfType<CodingAnswer>().FirstOrDefault(a => a.QuestionId == cq.Id);
                if (thisAns == null || string.IsNullOrWhiteSpace(thisAns.Code)) continue;

                // Fetch other coding answers for same question across attempts
                var otherAttempts = await _attempts.Find(a => a.AssessmentId == attempt.AssessmentId
                                                           && a.Id != attempt.Id
                                                           && a.Status == AttemptStatus.Submitted)
                                                   .ToListAsync();

                var otherCodes = otherAttempts
                    .Select(a => new { a.Id, Ans = a.Answers.OfType<CodingAnswer>().FirstOrDefault(x => x.QuestionId == cq.Id) })
                    .Where(x => x.Ans != null && !string.IsNullOrWhiteSpace(x.Ans!.Code))
                    .ToList();

                var normalized = NormalizeCode(thisAns.Code);
                double best = 0;
                List<string> similarIds = new();

                foreach (var oc in otherCodes)
                {
                    var normOther = NormalizeCode(oc.Ans!.Code);
                    var sim = JaccardSimilarity(Tokenize(normOther), Tokenize(normalized));
                    if (sim >= 0.80) similarIds.Add(oc.Id!); // threshold (tune as needed)
                    best = Math.Max(best, sim);
                }

                thisAns.PlagiarismScore = best;
                thisAns.SimilarAttemptIds = similarIds;

                if (best > 0)
                {
                    await _plagiarism.InsertOneAsync(new PlagiarismReport
                    {
                        AssessmentId = assessment.Id!,
                        QuestionId = cq.Id!,
                        AttemptId = attempt.Id!,
                        SimilarityScore = best,
                        SimilarDocuments = similarIds
                    });
                }

                // persist updated answer in attempt
                await _attempts.UpdateOneAsync(
                    x => x.Id == attempt.Id && x.Answers.Any(a => a.QuestionId == cq.Id),
                    Builders<Attempt>.Update.Set(a => a.Answers, attempt.Answers));
            }
        }

        // --- Simple, free plagiarism helpers (token Jaccard) ---
        private static string NormalizeCode(string code)
        {
            // Remove comments and excessive whitespace; lowercase
            code = Regex.Replace(code, @"//.*?$|/\*.*?\*/", "", RegexOptions.Singleline | RegexOptions.Multiline);
            code = Regex.Replace(code, @"\s+", " ");
            return code.Trim().ToLowerInvariant();
        }

        private static HashSet<string> Tokenize(string s, int k = 5)
        {
            // k-gram tokens over characters (very simple & language-agnostic)
            var set = new HashSet<string>();
            if (s.Length < k) { set.Add(s); return set; }
            for (int i = 0; i <= s.Length - k; i++)
                set.Add(s.Substring(i, k));
            return set;
        }

        private static double JaccardSimilarity(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 && b.Count == 0) return 1;
            int inter = a.Intersect(b).Count();
            int union = a.Union(b).Count();
            return union == 0 ? 0 : (double)inter / union;
        }
    }
}