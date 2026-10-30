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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedCourses;

public sealed record ListManagedCoursesQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListManagedCoursesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListManagedCoursesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListManagedCoursesQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.Courses.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(request.Search)) query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.OrderIndex).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.Title,x.Slug,x.Price,x.DiscountPrice,x.IsActive,x.IsPublished,x.AcademicHours}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListManagedCoursesQueryValidator : AbstractValidator<ListManagedCoursesQuery>
{
    public ListManagedCoursesQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
