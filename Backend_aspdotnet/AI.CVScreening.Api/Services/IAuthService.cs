using AI.CVScreening.Api.Models.Auth;

namespace AI.CVScreening.Api.Services;

public interface IAuthService
{
    AuthResponseDto SignUp(SignUpRequest request);
    AuthResponseDto Login(LoginRequest request);
    AuthResponseDto SignInWithGoogle(GoogleSignInRequest request);
    void Logout(string token);
}
