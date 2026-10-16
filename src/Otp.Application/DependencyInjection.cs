using Microsoft.Extensions.DependencyInjection;
using Otp.Application.Services;

namespace Otp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOtpService, OtpService>();
        return services;
    }
}