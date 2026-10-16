using System.ComponentModel.DataAnnotations;

namespace Otp.Application.Contracts;

public sealed record GenerateOtpRequest(
    [Required, StringLength(50, MinimumLength = 1)] string UserId,
    [Required] DateTime? DateTime);

public sealed record GenerateOtpResponse(string Otp, DateTime ExpiresAtUtc, int RemainingSeconds);

public sealed record ValidateOtpRequest(
    [Required, StringLength(50, MinimumLength = 1)] string UserId,
    [Required, RegularExpression(@"^\d{6}$", ErrorMessage = "Otp must be 6 digits.")] string Otp);

public sealed record ValidateOtpResponse(bool IsValid, string Status, string Message);

public sealed record OtpStatusResponse(bool IsActive, DateTime? ExpiresAtUtc, int RemainingSeconds);