using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicCertificates;

public sealed class ListAcademicCertificatesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListAcademicCertificatesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListAcademicCertificatesQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.DigitalCertificates.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseTitle.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Id).Select(x=>new {x.Id,x.EnrollmentId,x.StudentFullName,x.AcademicHours,x.QrValidationUrl,x.CertificateCode,x.UserId,x.CourseId,x.CourseTitle,x.FinalGrade,x.Status,x.IssueDate}),request.Page,request.PageSize,ct));
    }
}
