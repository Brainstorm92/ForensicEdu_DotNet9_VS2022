using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Infrastructure.BackgroundJobs;

public sealed class ExpiredExamProcessor(IApplicationDbContext db,IEvaluationService evaluations,
    IDatabaseResilience resilience,TimeProvider clock) : IExpiredExamProcessor
{
    public Task<int> ProcessAsync(CancellationToken cancellationToken) => resilience.ExecuteAsync(async token=>{
        var cutoff=clock.GetUtcNow().UtcDateTime.AddMinutes(-10);
        var expired=await db.ExamAttempts.AsNoTracking().Where(a=>a.CompletedAt==null && a.StartedAt<cutoff)
            .Select(a=>new {a.EnrollmentId,a.UserId}).ToListAsync(token);
        foreach(var attempt in expired)
            await evaluations.SubmitExamAttemptAsync(new ExamSubmissionDto(attempt.EnrollmentId,attempt.UserId,new(),600),token);
        return expired.Count;
    },cancellationToken);
}
