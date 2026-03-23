using System.ComponentModel.DataAnnotations;

namespace AI.CVScreening.Api.Models.Auth;

public sealed class SignUpRequest
{
    [Required]
    [MinLength(3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
