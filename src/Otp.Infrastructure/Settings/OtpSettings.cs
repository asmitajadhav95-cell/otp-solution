namespace Otp.Infrastructure.Settings;

public sealed class OtpSettings
{
    public const string SectionName = "Otp";
    public string Secret { get; set; } = string.Empty;
}