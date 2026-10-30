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
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicAdvisorys;

public sealed record ListPublicAdvisorysQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListPublicAdvisorysQueryHandler(IApplicationDbContext db) : IRequestHandler<ListPublicAdvisorysQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListPublicAdvisorysQuery request, CancellationToken ct)
    {
        var query=db.AdvisoryServices.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(request.Search)) query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Title).Select(x=>new { x.Id,x.Title,x.Specialist,x.Specialty,x.Description,x.Price }),request.Page,request.PageSize,ct));
    }
}

public sealed class ListPublicAdvisorysQueryValidator : AbstractValidator<ListPublicAdvisorysQuery>
{
    public ListPublicAdvisorysQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
