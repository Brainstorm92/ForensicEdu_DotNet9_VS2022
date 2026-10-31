using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicAttempts;

public sealed class ListAcademicAttemptsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListAcademicAttemptsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListAcademicAttemptsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.ExamAttempts.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseId.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Id).Select(x=>new {x.Id,x.EnrollmentId,x.UserId,x.CourseId,x.AttemptNumber,x.Score,x.Approved,x.Status,x.StartedAt,x.CompletedAt}),request.Page,request.PageSize,ct));
    }
}
