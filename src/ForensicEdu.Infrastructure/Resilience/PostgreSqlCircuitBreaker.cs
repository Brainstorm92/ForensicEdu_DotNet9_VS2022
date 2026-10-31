using ForensicEdu.Application.Common.Exceptions;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
namespace ForensicEdu.Infrastructure.Resilience;

public sealed class PostgreSqlCircuitBreaker : IDatabaseResilience
{
    private readonly ResiliencePipeline pipeline;
    private readonly TimeSpan breakDuration;

    public PostgreSqlCircuitBreaker(IOptions<DatabaseResilienceOptions> options,ITransientFailureClassifier classifier,
        TimeProvider timeProvider,ILogger<PostgreSqlCircuitBreaker> logger)
    {
        var settings=options.Value;
        breakDuration=TimeSpan.FromSeconds(settings.BreakDurationSeconds);
        pipeline=new ResiliencePipelineBuilder {TimeProvider=timeProvider}
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions {
                FailureRatio=settings.FailureRatio,
                MinimumThroughput=settings.MinimumThroughput,
                SamplingDuration=TimeSpan.FromSeconds(settings.SamplingDurationSeconds),
                BreakDuration=breakDuration,
                ShouldHandle=new PredicateBuilder().Handle<Exception>(classifier.IsTransient),
                OnOpened=args=>{logger.LogWarning("Circuito PostgreSQL abierto durante {Seconds} segundos.",args.BreakDuration.TotalSeconds);return default;},
                OnHalfOpened=_=>{logger.LogInformation("Circuito PostgreSQL en prueba de recuperación.");return default;},
                OnClosed=_=>{logger.LogInformation("Circuito PostgreSQL cerrado: servicio recuperado.");return default;}
            }).Build();
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken,Task<T>> operation,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try { return await pipeline.ExecuteAsync(async token=>await operation(token),cancellationToken); }
        catch(BrokenCircuitException ex) {throw new DatabaseCircuitOpenException(ex.RetryAfter??breakDuration,ex);}
    }
}
