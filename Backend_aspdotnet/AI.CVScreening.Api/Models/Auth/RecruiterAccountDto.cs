namespace AI.CVScreening.Api.Models.Auth;

public sealed class RecruiterAccountDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
