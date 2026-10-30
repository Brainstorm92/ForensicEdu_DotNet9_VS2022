using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;

namespace ForensicEdu.Infrastructure.BackgroundJobs;

public class AccountLifecycleHangfireJob
{
    private readonly IAccountLifecycleService _accountLifecycleService;

    public AccountLifecycleHangfireJob(IAccountLifecycleService accountLifecycleService)
    {
        _accountLifecycleService = accountLifecycleService;
    }

    public async Task ExecuteDailyLifecycleAuditAsync()
    {
        await _accountLifecycleService.ProcessDailyAccountExpirationsAsync();
    }
}
