using Learnlytics.API.Services;
using Learnlytics.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Learnlytics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssessmentsController : Controller
    {
        private readonly AssessmentService _service;

        public AssessmentsController(AssessmentService service)
        {
            _service = service;
        }

        [HttpGet("published")]
        [Authorize(Roles = "Instructor,Learner")]
        public async Task<IActionResult> GetPublishedAssessments()
        {
            var assessments = await _service.GetPublishedAsync();
            return Ok(assessments);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            var a = new Assessment()
            {
                Title = createAssessmentDto.Title,
                Description = createAssessmentDto.Description,
                Skills = createAssessmentDto.Skills,
                DurationMinutes = createAssessmentDto.DurationMinutes,
                Questions = createAssessmentDto.Questions,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            await _service.CreateAssessmentAsync(a);
            return Ok(a);
        }

        [HttpPost("{id}/publish")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Publish(string id, [FromQuery] bool published = true)
        {
            await _service.PublishAsync(id, published);
            return Ok(new {id, published});
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Learner,Instructor")]
        public async Task<IActionResult> Get(string id)
        {
            var a = await _service.GetByIdAsync(id);
            if (a == null) { return NotFound(); }
            return Ok(a);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateAssessment(string id, [FromBody] Assessment updatedAssessment)
        {
            if (id != updatedAssessment.Id)
                return BadRequest("Assessment ID mismatch.");

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Assessment with ID {id} not found.");

            await _service.UpdateAssessmentAsync(updatedAssessment);

            return Ok(updatedAssessment);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteAssessment(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Assessment with ID {id} not found.");

            await _service.DeleteAssessmentAsync(id);
            return NoContent();
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,Learner")]
        public async Task<ActionResult<List<Assessment>>> GetAllAssessments()
        {
            var assessments = await _service.GetAllAsync();
            return Ok(assessments);
        }

    }
}
