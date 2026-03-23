namespace AI.CVScreening.Api.Models.Auth;

public sealed class GoogleSignInRequest
{
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
