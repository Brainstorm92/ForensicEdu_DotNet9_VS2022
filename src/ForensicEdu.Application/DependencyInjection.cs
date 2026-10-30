using FluentValidation;
using MediatR;
using ForensicEdu.Application.Common.Behaviors;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Services;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(config => {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAntifraudService, AntifraudService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IAccountLifecycleService, AccountLifecycleService>();
        return services;
    }
}
