using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicCertificates;

public sealed record ListAcademicCertificatesQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListAcademicCertificatesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListAcademicCertificatesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListAcademicCertificatesQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.DigitalCertificates.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseTitle.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Id).Select(x=>new {x.Id,x.CertificateCode,x.UserId,x.CourseId,x.StudentFullName,x.CourseTitle,x.FinalGrade,x.Status,x.IssueDate}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListAcademicCertificatesQueryValidator : AbstractValidator<ListAcademicCertificatesQuery>
{
    public ListAcademicCertificatesQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
