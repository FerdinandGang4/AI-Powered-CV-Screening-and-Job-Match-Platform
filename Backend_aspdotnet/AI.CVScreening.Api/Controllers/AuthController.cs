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

    [HttpPost("login")]
    public ActionResult<AuthResponseDto> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = authService.Login(request);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("google")]
    public ActionResult<AuthResponseDto> SignInWithGoogle([FromBody] GoogleSignInRequest request)
    {
        try
        {
            var response = authService.SignInWithGoogle(request);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("logout")]
    public ActionResult Logout()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader["Bearer ".Length..].Trim()
            : Request.Headers["X-Recruiter-Token"].ToString();

        try
        {
            authService.Logout(token);
            return Ok(new { message = "Logged out successfully." });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
