using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicEnrollments;

public sealed class ListAcademicEnrollmentsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListAcademicEnrollmentsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListAcademicEnrollmentsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.Enrollments.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseId.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Id).Select(x=>new {x.Id,x.UserId,x.CourseId,x.OrderId,x.CompletedModulesJson,x.FirstAttemptScore,x.SecondAttemptScore,x.SecondAttemptAvailableAt,x.Status,x.ProgressPercent,x.ExamStatus,x.FinalGrade,x.AttemptsUsed,x.EnrolledAt}),request.Page,request.PageSize,ct));
    }
}
