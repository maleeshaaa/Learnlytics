using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Learnlytics.API.Models
{
    public enum AttemptStatus
    {
        InProgress,
        Submitted,
        Expired
    }

    public class Attempt
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssessmentId { get; set; } = null!;

        public string Username { get; set; } = null!;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiredAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

        public List<Answers> Answers { get; set; } = new();

        public int AutoScore { get; set; } = 0;
        public int ManualScore { get; set; } = 0;
        public int TotalScore => AutoScore + ManualScore;
    }

    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(McqAnswer), typeof(CodingAnswer))]
    public abstract class Answers
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? QuestionId { get; set; } = null!;
        public QuestionType QuestionType { get; set; }
    }

    public class McqAnswer : Answers
    {
        public List<int> SelectedOptions { get; set; } = new();
        public bool IsCorrect { get; set; } = false;
        public McqAnswer()
        {
            QuestionType = QuestionType.MCQ;
        }
    }

    public class CodingAnswer : Answers
    {
        public string Code { get; set; } = string.Empty;
        public double? PlagiarismScore { get; set; }
        public bool IsCorrect { get; set; } = false;
        public List<string> SimilarAttemptIds { get; set; } = new();
        public CodingAnswer()
        {
            QuestionType = QuestionType.Coding;
        }
    }
}
