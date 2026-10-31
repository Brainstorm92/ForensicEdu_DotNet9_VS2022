using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyCertificates;

public sealed class ListMyCertificatesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListMyCertificatesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListMyCertificatesQuery request, CancellationToken ct)
    {
        var query=db.DigitalCertificates.AsNoTracking().Where(x=>x.UserId==current.UserId);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseTitle.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.IssueDate).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.EnrollmentId,x.StudentFullName,x.AcademicHours,x.QrValidationUrl,x.CertificateCode,x.CourseId,x.CourseTitle,x.FinalGrade,x.Status,x.IssueDate}),request.Page,request.PageSize,ct));
    }
}
