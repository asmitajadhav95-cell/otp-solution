using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Otp.Application.Contracts;
using Otp.Application.Services;
using Otp.Domain.Exceptions;
using Otp.Infrastructure.Generators;
using Otp.Infrastructure.Persistence;
using Otp.Infrastructure.Settings;

namespace Otp.UnitTests;

public class OtpServiceTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 16, 6, 0, 0, TimeSpan.Zero));
    private readonly OtpService _sut;

    public OtpServiceTests()
    {
        var settings = Options.Create(new OtpSettings { Secret = new string('x', 32) });
        _sut = new OtpService(new HmacOtpGenerator(settings), new InMemoryOtpRepository(), _clock);
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;
    private GenerateOtpResponse Generate() => _sut.Generate(new GenerateOtpRequest("tushar", Now));
    private ValidateOtpResponse Validate(string otp) => _sut.Validate(new ValidateOtpRequest("tushar", otp));

    [Fact]
    public void Generate_returns_six_digits_valid_for_30_seconds()
    {
        var result = Generate();
        Assert.Matches(@"^\d{6}$", result.Otp);
        Assert.Equal(30, result.RemainingSeconds);
    }

    [Fact]
    public void Correct_code_works_only_once()
    {
        var otp = Generate().Otp;
        Assert.True(Validate(otp).IsValid);
        Assert.Equal("NotFound", Validate(otp).Status);
    }

    [Fact]
    public void Code_expires_after_30_seconds()
    {
        var otp = Generate().Otp;
        _clock.Advance(TimeSpan.FromSeconds(30));   // no real waiting
        Assert.Equal("Expired", Validate(otp).Status);
    }

    [Fact]
    public void Three_wrong_attempts_lock_the_code()
    {
        var otp = Generate().Otp;
        var wrong = otp == "000000" ? "111111" : "000000";

        Assert.Equal("Invalid", Validate(wrong).Status);
        Assert.Equal("Invalid", Validate(wrong).Status);
        Assert.Equal("Locked", Validate(wrong).Status);
        Assert.False(Validate(otp).IsValid);        // even the right code is dead now
    }

    [Fact]
    public void DateTime_older_than_30_seconds_is_rejected() =>
        Assert.Throws<DomainException>(() => _sut.Generate(new GenerateOtpRequest("tushar", Now.AddSeconds(-31))));

    [Fact]
    public void Future_DateTime_is_rejected() =>
        Assert.Throws<DomainException>(() => _sut.Generate(new GenerateOtpRequest("tushar", Now.AddMinutes(1))));
}