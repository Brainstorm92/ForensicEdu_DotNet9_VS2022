using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace ForensicEdu.Infrastructure.Resilience;

public static class ResilienceServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseResilience(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddOptions<DatabaseResilienceOptions>().Bind(configuration.GetSection(DatabaseResilienceOptions.SectionName))
            .Validate(x=>x.FailureRatio>0 && x.FailureRatio<=1,"FailureRatio debe estar entre 0 y 1.")
            .Validate(x=>x.MinimumThroughput>=2,"MinimumThroughput debe ser al menos 2.")
            .Validate(x=>x.SamplingDurationSeconds is >=1 and <=300,"SamplingDurationSeconds debe estar entre 1 y 300.")
            .Validate(x=>x.BreakDurationSeconds is >=1 and <=300,"BreakDurationSeconds debe estar entre 1 y 300.")
            .ValidateOnStart();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<ITransientFailureClassifier,NpgsqlTransientFailureClassifier>();
        // One circuit per process, never one circuit per HTTP request/DbContext.
        services.TryAddSingleton<IDatabaseResilience,PostgreSqlCircuitBreaker>();
        return services;
    }
}
