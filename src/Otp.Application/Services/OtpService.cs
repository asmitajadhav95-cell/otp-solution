using Otp.Application.Abstractions;
using Otp.Application.Contracts;
using Otp.Domain.Entities;
using Otp.Domain.Enums;

namespace Otp.Application.Services;

// Every dependency arrives through the constructor = Dependency Injection.
public sealed class OtpService(
    IOtpGenerator generator,
    IOtpRepository repository,
    TimeProvider clock) : IOtpService
{
    public GenerateOtpResponse Generate(GenerateOtpRequest request)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var userId = request.UserId.Trim();
        var issuedAt = ToUtc(request.DateTime!.Value);

        var code = generator.Generate(userId, issuedAt);                 // 1. make the code
        var otp = OneTimePassword.Create(userId, code, issuedAt, now);   // 2. apply business rules
        repository.Save(otp);                                            // 3. store (replaces old one)

        return new GenerateOtpResponse(otp.Code, otp.ExpiresAtUtc, otp.RemainingSeconds(now));
    }

    public ValidateOtpResponse Validate(ValidateOtpRequest request)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var userId = request.UserId.Trim();

        var otp = repository.Get(userId);
        if (otp is null)
            return new(false, "NotFound", "No active password for this user. Generate a new one.");

        var result = otp.Verify(request.Otp.Trim(), now);   // Domain decides

        // Anything except a plain wrong guess ends this OTP's life.
        if (result != OtpValidationResult.Invalid)
            repository.Remove(userId);

        return new(result == OtpValidationResult.Valid, result.ToString(), MessageFor(result));
    }

    public OtpStatusResponse GetStatus(string userId)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var otp = repository.Get(userId.Trim());

        return otp is null || otp.IsUsed || otp.IsExpired(now)
            ? new(false, null, 0)
            : new(true, otp.ExpiresAtUtc, otp.RemainingSeconds(now));
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string MessageFor(OtpValidationResult result) => result switch
    {
        OtpValidationResult.Valid => "Password is valid.",
        OtpValidationResult.Invalid => "Password is incorrect.",
        OtpValidationResult.Expired => "Password has expired. Generate a new one.",
        OtpValidationResult.Locked => "Too many wrong attempts. Generate a new one.",
        OtpValidationResult.AlreadyUsed => "Password was already used.",
        _ => "Unknown result."
    };
}