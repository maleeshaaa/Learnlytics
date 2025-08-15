using System.Text.Json.Serialization;

namespace Learnlytics.API.Models
{
    public class UserDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class CreateAssessmentDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public int DurationMinutes { get; set; } = 30;
        public List<QuestionBase> Questions { get; set; } = new();
    }

    public class StartAttemptDto
    {
        public string AssessmentId { get; set; } = null!;
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(McqAnswerDto), "mcq")]
    [JsonDerivedType(typeof(CodingAnswerDto), "coding")]
    public abstract class AnswerDto
    {
        public string QuestionId { get; set; } = null!;
        public QuestionType QuestionType { get; set; }
    }

    public class McqAnswerDto : AnswerDto
    {
        public List<int> SelectedOptions { get; set; } = new();
        public McqAnswerDto()
        {
            QuestionType = QuestionType.MCQ;
        }
    }

    public class CodingAnswerDto : AnswerDto
    {
        public string Code { get; set; } = string.Empty;
        public CodingAnswerDto()
        {
            QuestionType = QuestionType.Coding;
        }
    }

    public class SubmitAttemptDto
    {
        public string AttemptId { get; set; } = null!;
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
