using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Learnlytics.API.Models
{
    public class PlagiarismReport
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string AssessmentId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string AttemptId { get; set; } = null!;

        public double SimilarityScore { get; set; }
        public List<string> SimilarDocuments { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}
