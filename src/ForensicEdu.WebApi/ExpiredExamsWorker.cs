using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi;

// Grades the last saved answers even when the student's browser was closed.
public class ExpiredExamsWorker(IServiceScopeFactory scopes, ILogger<ExpiredExamsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                var expired = await db.ExamAttempts.AsNoTracking().Where(a => a.CompletedAt == null && a.StartedAt < cutoff)
                    .Select(a => new { a.EnrollmentId, a.UserId }).ToListAsync(stoppingToken);
                foreach (var attempt in expired)
                {
                    using var attemptScope = scopes.CreateScope();
                    await attemptScope.ServiceProvider.GetRequiredService<IEvaluationService>()
                        .SubmitExamAttemptAsync(new ExamSubmissionDto(attempt.EnrollmentId, attempt.UserId, new(), 600), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "No se pudo cerrar las evaluaciones vencidas. Compruebe la base de datos."); }
        }
    }
}
