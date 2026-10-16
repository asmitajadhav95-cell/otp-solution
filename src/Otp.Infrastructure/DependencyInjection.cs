using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Otp.Application.Abstractions;
using Otp.Infrastructure.Generators;
using Otp.Infrastructure.Persistence;
using Otp.Infrastructure.Settings;

namespace Otp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OtpSettings>()
            .Bind(configuration.GetSection(OtpSettings.SectionName))
            .Validate(s => s.Secret.Length >= 32, "Otp:Secret must be at least 32 characters.")
            .ValidateOnStart(); // app refuses to start with a weak/missing secret

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOtpGenerator, HmacOtpGenerator>();
        services.AddSingleton<IOtpRepository, InMemoryOtpRepository>(); // singleton: one shared store
        return services;
    }
}