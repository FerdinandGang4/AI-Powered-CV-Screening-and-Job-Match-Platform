using AI.CVScreening.Api.Models.Auth;

namespace AI.CVScreening.Api.Services;

public interface IAuthService
{
    AuthResponseDto SignUp(SignUpRequest request);
}
