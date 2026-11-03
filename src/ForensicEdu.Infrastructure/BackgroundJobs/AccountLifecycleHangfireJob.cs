using ForensicEdu.Application.Common.Interfaces;
namespace ForensicEdu.Infrastructure.BackgroundJobs;

public sealed class AccountLifecycleHangfireJob(IAccountLifecycleService lifecycle,IDatabaseResilience resilience)
{
    [Hangfire.DisableConcurrentExecution(300)]
    public Task ExecuteDailyLifecycleAuditAsync() => resilience.ExecuteAsync(async ct=>{
        await lifecycle.ProcessDailyAccountExpirationsAsync(ct);return true;
    });
}
