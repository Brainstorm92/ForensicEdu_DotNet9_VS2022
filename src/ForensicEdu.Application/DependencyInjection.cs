using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ForensicEdu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAntifraudService, AntifraudService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IAccountLifecycleService, AccountLifecycleService>();

        return services;
    }
}
