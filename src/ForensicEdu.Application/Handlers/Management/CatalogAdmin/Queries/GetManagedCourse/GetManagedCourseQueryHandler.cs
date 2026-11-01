using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedCourse;

public sealed class GetManagedCourseQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetManagedCourseQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetManagedCourseQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.Courses.AsNoTracking().Include(x=>x.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        if(item==null)return ApplicationResult.NotFound();
        return ApplicationResult.Success(new { item.Id,item.Title,item.Slug,item.Category,item.Price,item.DiscountPrice,item.AcademicHours,item.Instructor,item.InstructorImageUrl,item.ShortDescription,item.FullDescription,item.ImageUrl,item.Modalidad,item.Requisitos,item.OrderIndex,item.IsPublished,item.IsActive,item.ShortCode,
            modules=item.Modules.OrderBy(m=>m.OrderIndex).Select(m=>new {m.Id,m.Title,m.OrderIndex,m.Duration,m.VideoStreamingUrl,hasPrivateVideo=m.StreamingId.StartsWith("videos/"),documents=m.Documents.Select(d=>new {d.Id,d.Title,d.FileType,d.FileSize,d.StorageUrl,d.RequireWatermark})}) });
    }
}
