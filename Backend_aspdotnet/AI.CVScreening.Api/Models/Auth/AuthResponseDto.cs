namespace AI.CVScreening.Api.Models.Auth;

public sealed class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
