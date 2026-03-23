using AI.CVScreening.Api.Models.Auth;
using System.Security.Cryptography;
using System.Text;

namespace AI.CVScreening.Api.Services;

public sealed class InMemoryAuthService(AppMemoryStore store) : IAuthService
{
    public AuthResponseDto SignUp(SignUpRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (store.RecruiterAccounts.Any(account =>
                string.Equals(account.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var account = new RecruiterAccountDto
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            CompanyName = request.CompanyName.Trim(),
            Email = normalizedEmail,
            PasswordHash = HashPassword(request.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        store.RecruiterAccounts.Add(account);

        return new AuthResponseDto
        {
            UserId = account.Id,
            FullName = account.FullName,
            CompanyName = account.CompanyName,
            Email = account.Email,
            CreatedAtUtc = account.CreatedAtUtc,
            Message = "Account created successfully. You can now use the platform as a recruiter."
        };
    }

    public AuthResponseDto Login(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var account = store.RecruiterAccounts.FirstOrDefault(existing =>
            string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (account is null || !string.Equals(account.PasswordHash, HashPassword(request.Password), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        return new AuthResponseDto
        {
            UserId = account.Id,
            FullName = account.FullName,
            CompanyName = account.CompanyName,
            Email = account.Email,
            CreatedAtUtc = account.CreatedAtUtc,
            Message = $"Welcome back, {account.FullName}."
        };
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
