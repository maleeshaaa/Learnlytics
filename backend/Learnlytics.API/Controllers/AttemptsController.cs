using Learnlytics.API.Models;
using Learnlytics.API.Services;
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
            await _service.SubmitAnswerAsync(submitAttemptDto.AttemptId, submitAttemptDto.Answers);
            var attempt = await _service.GetAttemptAsync(submitAttemptDto.AttemptId);
            return Ok(new { attempt!.TotalScore, attempt.AutoScore, attempt.ManualScore, attempt.Status });
        }
    }
}
