using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Policies;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Enrollments;

public sealed class StudentServicesEnrollmentsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesEnrollmentsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesEnrollmentsQuery message, CancellationToken ct)
    {
        var rows=await db.Enrollments.AsNoTracking().Where(e=>e.UserId==current.UserId)
            .OrderByDescending(e=>e.EnrolledAt).ThenByDescending(e=>e.Id).ToListAsync(ct);
        var exhausted=await db.Enrollments.Where(e=>e.UserId==current.UserId).Where(EnrollmentPolicy.Exhausted).Select(e=>e.Id).ToListAsync(ct);
        var eligibleCourses=rows.GroupBy(e=>e.CourseId).Where(g=>g.All(e=>exhausted.Contains(e.Id))).Select(g=>g.Key).ToHashSet();
        var latest=rows.GroupBy(e=>e.CourseId).Select(g=>g.First().Id).ToHashSet();
        return ApplicationResult.Success(rows.Select(e=>new {e.Id,e.UserId,e.OrderId,e.EnrolledAt,e.CompletedModulesJson,e.FirstAttemptScore,e.SecondAttemptScore,e.CourseId,e.Status,e.ProgressPercent,e.ExamStatus,e.FinalGrade,e.AttemptsUsed,e.SecondAttemptAvailableAt,
            canReenroll=latest.Contains(e.Id)&&eligibleCourses.Contains(e.CourseId)}));
    }
}
