using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetMyExamHistory;

public sealed class GetMyExamHistoryQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetMyExamHistoryQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetMyExamHistoryQuery request, CancellationToken ct)
    {
        if(!await db.Enrollments.AnyAsync(x=>x.Id==request.Id && x.UserId==current.UserId,ct))return ApplicationResult.NotFound();
        return ApplicationResult.Success(await db.ExamAttempts.AsNoTracking().Where(x=>x.EnrollmentId==request.Id && x.UserId==current.UserId).OrderBy(x=>x.AttemptNumber)
            .Select(x=>new {x.Id,x.AttemptNumber,x.Score,x.Approved,x.Status,x.StartedAt,x.CompletedAt,x.NextAttemptAllowedAt}).ToListAsync(ct));
    }
}
