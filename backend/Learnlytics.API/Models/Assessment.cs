using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Learnlytics.API.Models
{
    public enum QuestionType
    {
        MCQ,
        Coding
    }

    public class Assessment
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Title { get; set; } = null;
        public string Description { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public int DurationMinutes { get; set; } = 30;
        public bool Published { get; set; } = false;

        public List<QuestionBase> Questions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "";
    }

    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(McqQuestion), typeof(CodingQuestion))]
    public abstract class QuestionBase
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public QuestionType QuestionType { get; set; }
        public string Prompt { get; set; } = null!;
        public int Points { get; set; } = 1;
    }

    public class McqQuestion : QuestionBase
    {
        public List<string> Options { get; set; } = new();
        public List<int> CorrectAnswers { get; set; } = new();
        public bool ShuffleOptions { get; set; } = true;
        public McqQuestion()
        {
            QuestionType = QuestionType.MCQ;
        }
    }

    public class CodingQuestion : QuestionBase
    {
        public string? Language { get; set; } = "JavaScript";
        public string? StarterCode { get; set; }
        public List<string> TestCases { get; set; } = new();
        public string? ExpectedOutput { get; set; }
        public string PlagiarismGroupKey { get; set; } = "";
        public CodingQuestion()
        {
            QuestionType = QuestionType.Coding;
        }
    }
}
