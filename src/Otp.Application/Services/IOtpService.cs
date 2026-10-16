using Otp.Application.Contracts;

namespace Otp.Application.Services;

public interface IOtpService
{
    GenerateOtpResponse Generate(GenerateOtpRequest request);
    ValidateOtpResponse Validate(ValidateOtpRequest request);
    OtpStatusResponse GetStatus(string userId);
}