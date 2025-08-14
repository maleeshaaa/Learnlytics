using Learnlytics.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Learnlytics.API.Services
{
    public class AssessmentService
    {
        private readonly IMongoCollection<Assessment> _assessments;

        public AssessmentService(IOptions<MongoSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _assessments = database.GetCollection<Assessment>("Assessments");

            _assessments.Indexes.CreateOne(
                new CreateIndexModel<Assessment>(
                    Builders<Assessment>.IndexKeys.Ascending(a => a.Published))); // Index to optimize published assessments queries
        }

        // Get all assessments
        public Task<List<Assessment>> GetAllAsync()
        {
            return _assessments.Find(a => true).ToListAsync();
        }

        // Get all published assessments
        public Task<List<Assessment>> GetPublishedAsync()
        {
            return _assessments.Find(a => a.Published).ToListAsync();
        }

        // Get assessment by Id
        public Task<Assessment?> GetByIdAsync(string id)
        {
            return _assessments.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        // Create a new assessment
        public Task CreateAssessmentAsync(Assessment a)
        {
            return _assessments.InsertOneAsync(a);
        }

        // Update an existing assessment
        public Task UpdateAssessmentAsync(Assessment a)
        {
            return _assessments.ReplaceOneAsync(x => x.Id == a.Id, a);
        }

        // Delete an assessment
        public Task DeleteAssessmentAsync(string id)
        {
            return _assessments.DeleteOneAsync(x => x.Id == id);
        }

        // Update the published status of an assessment
        public Task PublishAsync(string id, bool published)
        {
            return _assessments.UpdateOneAsync(
                x => x.Id == id,
                Builders<Assessment>.Update.Set(x => x.Published, published));
        }
    }
}
