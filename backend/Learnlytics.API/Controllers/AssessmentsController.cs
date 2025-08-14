using Learnlytics.API.Services;
using Learnlytics.API.Models;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetPublishedAssessments()
        {
            var assessments = await _service.GetPublishedAsync();
            return Ok(assessments);
        }

        [HttpPost]
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
        public async Task<IActionResult> Publish(string id, [FromQuery] bool published = true)
        {
            await _service.PublishAsync(id, published);
            return Ok(new {id, published});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var a = await _service.GetByIdAsync(id);
            if (a == null) { return NotFound(); }
            return Ok(a);
        }

        [HttpPut("{id}")]
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
        public async Task<IActionResult> DeleteAssessment(string id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Assessment with ID {id} not found.");

            await _service.DeleteAssessmentAsync(id);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<List<Assessment>>> GetAllAssessments()
        {
            var assessments = await _service.GetAllAsync();
            return Ok(assessments);
        }

    }
}
