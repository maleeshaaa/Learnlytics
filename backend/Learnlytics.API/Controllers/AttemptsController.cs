using Learnlytics.API.Models;
using Learnlytics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnlytics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttemptsController : Controller
    {
        private readonly AttemptService _service;

        public AttemptsController(AttemptService service) 
        { 
            _service = service;
        }

        [Authorize(Roles = "Learner")]
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartAttemptDto startAttemptDto)
        {
            var username = User.Identity?.Name ?? throw new InvalidOperationException("No Identity");
            var attempt = await _service.StartAttemptAsync(startAttemptDto.AssessmentId, username);
            return Ok(attempt);
        }

        [HttpGet("{attemptId}")]
        public async Task<IActionResult> Get(string attemptId)
        {
            var attempt = await _service.GetAttemptAsync(attemptId);
            if (attempt == null)
                return NotFound();
            return Ok(attempt);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitAttemptDto submitAttemptDto)
        {
            // Map DTOs to your domain Answer classes
            var answers = submitAttemptDto.Answers.Select(a => a.QuestionType switch
            {
                QuestionType.MCQ => new McqAnswer
                {
                    QuestionId = a.QuestionId,
                    SelectedOptions = ((McqAnswerDto)a).SelectedOptions
                } as Answers,
                QuestionType.Coding => new CodingAnswer
                {
                    QuestionId = a.QuestionId,
                    Code = ((CodingAnswerDto)a).Code
                } as Answers,
                _ => throw new ArgumentException("Unknown question type")
            }).ToList();

            // Submit answers in the service
            await _service.SubmitAnswerAsync(submitAttemptDto.AttemptId, answers);

            // Get updated attempt
            var attempt = await _service.GetAttemptAsync(submitAttemptDto.AttemptId);

            return Ok(new
            {
                attempt!.TotalScore,
                attempt.AutoScore,
                attempt.ManualScore,
                attempt.Status
            });
        }
    }
}
