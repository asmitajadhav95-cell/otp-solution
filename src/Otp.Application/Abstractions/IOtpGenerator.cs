namespace Otp.Application.Abstractions;

public interface IOtpGenerator
{
    string Generate(string userId, DateTime issuedAtUtc);
}