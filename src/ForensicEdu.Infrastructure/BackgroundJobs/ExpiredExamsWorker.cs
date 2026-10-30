using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace ForensicEdu.Infrastructure.BackgroundJobs;

public sealed class ExpiredExamsWorker(IServiceScopeFactory scopes,TimeProvider clock,ILogger<ExpiredExamsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(15),clock);
        try
        {
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope=scopes.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IExpiredExamProcessor>().ProcessAsync(stoppingToken);
                }
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) {break;}
                catch(Exception exception) {logger.LogError(exception,"No se pudieron cerrar las evaluaciones vencidas.");}
            }
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { }
    }
}
