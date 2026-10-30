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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicAttempts;

public sealed record ListAcademicAttemptsQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListAcademicAttemptsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListAcademicAttemptsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListAcademicAttemptsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.ExamAttempts.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.CourseId.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Id).Select(x=>new {x.Id,x.EnrollmentId,x.UserId,x.CourseId,x.AttemptNumber,x.Score,x.Approved,x.Status,x.StartedAt,x.CompletedAt}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListAcademicAttemptsQueryValidator : AbstractValidator<ListAcademicAttemptsQuery>
{
    public ListAcademicAttemptsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
