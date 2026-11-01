using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure.Services;
public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformEmail(this IServiceCollection services)
    {
        services.AddScoped<IPlatformEmail,GmailPlatformEmail>();
        services.AddScoped<NotificationEmailDispatcher>();
        services.AddScoped<ForensicEdu.Infrastructure.BackgroundJobs.NotificationEmailJob>();
        services.AddScoped<IEmailConfirmationTokens,EmailConfirmationTokens>();
        services.AddScoped<IEmailConfirmation,ForensicEdu.Application.Services.EmailConfirmationService>();
        services.AddScoped<IPasswordRecoveryTokens,PasswordRecoveryTokens>();
        services.AddScoped<IPasswordRecovery,ForensicEdu.Application.Services.PasswordRecoveryService>();
        return services;
    }
}
