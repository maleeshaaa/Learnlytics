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
            if (submitAttemptDto == null || submitAttemptDto.Answers == null || !submitAttemptDto.Answers.Any())
                return BadRequest("No answers provided.");

            var answers = new List<Answers>();

            foreach (var a in submitAttemptDto.Answers)
            {
                switch (a.QuestionType)
                {
                    case QuestionType.MCQ:
                        if (a is McqAnswerDto mcqDto)
                        {
                            answers.Add(new McqAnswer
                            {
                                QuestionId = mcqDto.QuestionId,
                                SelectedOptions = mcqDto.SelectedOptions
                            });
                        }
                        else
                        {
                            return BadRequest("Invalid MCQ answer format.");
                        }
                        break;

                    case QuestionType.Coding:
                        if (a is CodingAnswerDto codingDto)
                        {
                            answers.Add(new CodingAnswer
                            {
                                QuestionId = codingDto.QuestionId,
                                Code = codingDto.Code
                            });
                        }
                        else
                        {
                            return BadRequest("Invalid Coding answer format.");
                        }
                        break;

                    default:
                        return BadRequest("Unknown question type.");
                }
            }

            // Submit answers in the service
            await _service.SubmitAnswerAsync(submitAttemptDto.AttemptId, answers);

            // Get updated attempt
            var attempt = await _service.GetAttemptAsync(submitAttemptDto.AttemptId);
            if (attempt == null)
                return NotFound("Attempt not found.");

            return Ok(new
            {
                attempt.TotalScore,
                attempt.AutoScore,
                attempt.ManualScore,
                attempt.Status
            });
        }
    }
}
