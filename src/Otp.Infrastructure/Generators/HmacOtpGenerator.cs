using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Otp.Application.Abstractions;
using Otp.Infrastructure.Settings;

namespace Otp.Infrastructure.Generators;

public sealed class HmacOtpGenerator(IOptions<OtpSettings> settings) : IOtpGenerator
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(settings.Value.Secret);

    public string Generate(string userId, DateTime issuedAtUtc)
    {
        // HMAC = hash keyed with a server secret → output can't be guessed without the secret.
        var message = Encoding.UTF8.GetBytes($"{userId}|{issuedAtUtc.Ticks}");
        var hash = HMACSHA256.HashData(_key, message);

        // RFC 4226 "dynamic truncation" (same as Google Authenticator) → 6 digits
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | (hash[offset + 1] << 16)
                   | (hash[offset + 2] << 8)
                   |  hash[offset + 3];

        return (binary % 1_000_000).ToString("D6");
    }
}