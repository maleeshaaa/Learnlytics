using Microsoft.AspNetCore.Mvc;

namespace Learnlytics.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "OK", message = "Learnlytics backend is running!" });
        }
    }
}
