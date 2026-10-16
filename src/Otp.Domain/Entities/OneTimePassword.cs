using System.Security.Cryptography;
using System.Text;
using Otp.Domain.Enums;
using Otp.Domain.Exceptions;

namespace Otp.Domain.Entities;

public sealed class OneTimePassword
{
    public const int ValiditySeconds = 30;
    public const int MaxFailedAttempts = 3;
    private const int AllowedClockSkewSeconds = 5;

    private readonly Lock _sync = new();

    public string UserId { get; }
    public string Code { get; }
    public DateTime ExpiresAtUtc { get; }
    public int FailedAttempts { get; private set; }
    public bool IsUsed { get; private set; }

    private OneTimePassword(string userId, string code, DateTime expiresAtUtc)
    {
        UserId = userId;
        Code = code;
        ExpiresAtUtc = expiresAtUtc;
    }

    // Factory method: the ONLY way to create an OTP, so the rules can't be skipped.
    public static OneTimePassword Create(string userId, string code, DateTime issuedAtUtc, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId is required.");

        if (issuedAtUtc > nowUtc.AddSeconds(AllowedClockSkewSeconds))
            throw new DomainException("DateTime cannot be in the future.");

        var expiresAt = issuedAtUtc.AddSeconds(ValiditySeconds);
        if (expiresAt <= nowUtc)
            throw new DomainException($"DateTime is more than {ValiditySeconds} seconds old; the password would already be expired.");

        // Never live longer than 30s from now, even with clock skew.
        var latestAllowed = nowUtc.AddSeconds(ValiditySeconds);
        if (expiresAt > latestAllowed) expiresAt = latestAllowed;

        return new OneTimePassword(userId, code, expiresAt);
    }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;

    public int RemainingSeconds(DateTime nowUtc) =>
        Math.Max(0, (int)Math.Ceiling((ExpiresAtUtc - nowUtc).TotalSeconds));

    public OtpValidationResult Verify(string code, DateTime nowUtc)
    {
        lock (_sync) // two requests at the same moment can't both "use" it
        {
            if (IsUsed) return OtpValidationResult.AlreadyUsed;
            if (IsExpired(nowUtc)) return OtpValidationResult.Expired;
            if (FailedAttempts >= MaxFailedAttempts) return OtpValidationResult.Locked;

            // Constant-time compare: response time doesn't leak how many digits matched.
            var matches = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(Code), Encoding.UTF8.GetBytes(code));

            if (matches)
            {
                IsUsed = true;
                return OtpValidationResult.Valid;
            }

            FailedAttempts++;
            return FailedAttempts >= MaxFailedAttempts
                ? OtpValidationResult.Locked
                : OtpValidationResult.Invalid;
        }
    }
}