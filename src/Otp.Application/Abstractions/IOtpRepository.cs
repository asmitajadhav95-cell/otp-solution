using Otp.Domain.Entities;

namespace Otp.Application.Abstractions;

public interface IOtpRepository
{
    void Save(OneTimePassword otp);
    OneTimePassword? Get(string userId);
    void Remove(string userId);
}