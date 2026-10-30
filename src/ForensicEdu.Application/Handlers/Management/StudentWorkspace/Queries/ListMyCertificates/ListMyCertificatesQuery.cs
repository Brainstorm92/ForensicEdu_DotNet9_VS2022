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
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyCertificates;

public sealed record ListMyCertificatesQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListMyCertificatesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListMyCertificatesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListMyCertificatesQuery request, CancellationToken ct)
    {
        var query=db.DigitalCertificates.AsNoTracking().Where(x=>x.UserId==current.UserId);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseTitle.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.IssueDate).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.CertificateCode,x.CourseId,x.CourseTitle,x.AcademicHours,x.FinalGrade,x.Status,x.IssueDate,x.QrValidationUrl}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListMyCertificatesQueryValidator : AbstractValidator<ListMyCertificatesQuery>
{
    public ListMyCertificatesQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
