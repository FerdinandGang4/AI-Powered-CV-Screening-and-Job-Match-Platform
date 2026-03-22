using Microsoft.AspNetCore.Mvc;

namespace AI.CVScreening.Api.Controllers;

public sealed class HealthController : BaseApiController
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "AI.CVScreening.Api",
            utcTime = DateTime.UtcNow
        });
    }
}
