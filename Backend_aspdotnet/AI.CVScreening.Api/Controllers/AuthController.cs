using AI.CVScreening.Api.Models.Auth;
using AI.CVScreening.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.CVScreening.Api.Controllers;

public sealed class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("signup")]
    public ActionResult<AuthResponseDto> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var response = authService.SignUp(request);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
