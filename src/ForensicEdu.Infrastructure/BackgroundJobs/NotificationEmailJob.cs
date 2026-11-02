using ForensicEdu.Infrastructure.Services;
using Hangfire;
namespace ForensicEdu.Infrastructure.BackgroundJobs;
public sealed class NotificationEmailJob(NotificationEmailDispatcher dispatcher)
{
    [DisableConcurrentExecution(300)]
    public Task ExecuteAsync() => dispatcher.DispatchAsync(CancellationToken.None);
}
