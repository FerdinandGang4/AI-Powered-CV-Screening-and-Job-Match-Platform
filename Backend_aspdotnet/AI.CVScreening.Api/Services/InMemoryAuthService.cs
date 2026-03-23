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
            Token = string.Empty,
            CreatedAtUtc = account.CreatedAtUtc,
            Message = "Account created successfully. Please log in to use the platform."
        };
    }

    public AuthResponseDto SignInWithGoogle(GoogleSignInRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Please use a valid Gmail address to continue with Gmail.");
        }

        var account = store.RecruiterAccounts.FirstOrDefault(existing =>
            string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (account is null)
        {
            account = new RecruiterAccountDto
            {
                Id = Guid.NewGuid(),
                FullName = string.IsNullOrWhiteSpace(request.FullName)
                    ? BuildDisplayNameFromEmail(normalizedEmail)
                    : request.FullName.Trim(),
                CompanyName = string.IsNullOrWhiteSpace(request.CompanyName)
                    ? "Independent Recruiter"
                    : request.CompanyName.Trim(),
                Email = normalizedEmail,
                PasswordHash = string.Empty,
                CreatedAtUtc = DateTime.UtcNow
            };

            store.RecruiterAccounts.Add(account);
        }

        var token = CreateSessionToken(account.Id);

        return new AuthResponseDto
        {
            UserId = account.Id,
            FullName = account.FullName,
            CompanyName = account.CompanyName,
            Email = account.Email,
            Token = token,
            CreatedAtUtc = account.CreatedAtUtc,
            Message = $"Signed in with Gmail as {account.FullName}."
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

        var token = CreateSessionToken(account.Id);

        return new AuthResponseDto
        {
            UserId = account.Id,
            FullName = account.FullName,
            CompanyName = account.CompanyName,
            Email = account.Email,
            Token = token,
            CreatedAtUtc = account.CreatedAtUtc,
            Message = $"Welcome back, {account.FullName}."
        };
    }

    public void Logout(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("A valid authentication token is required to log out.");
        }

        if (!store.RecruiterSessions.Remove(token))
        {
            throw new InvalidOperationException("The current session is already signed out or invalid.");
        }
    }

    private string CreateSessionToken(Guid userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        store.RecruiterSessions[token] = userId;
        return token;
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static string BuildDisplayNameFromEmail(string email)
    {
        var localPart = email.Split('@')[0];
        var normalized = localPart.Replace('.', ' ').Replace('_', ' ').Replace('-', ' ').Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? "Recruiter"
            : string.Join(' ', normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }
}
