using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedCourses;

public sealed class ListManagedCoursesQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListManagedCoursesQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListManagedCoursesQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.Courses.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(request.Search)) query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.OrderIndex).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.Title,x.Slug,x.Price,x.DiscountPrice,x.IsActive,x.IsPublished,x.AcademicHours,x.Instructor,x.Category,x.ShortCode}),request.Page,request.PageSize,ct));
    }
}
