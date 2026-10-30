using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.ArchiveCourse;

public sealed class ArchiveCourseCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ArchiveCourseCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ArchiveCourseCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.Courses.FirstOrDefaultAsync(x=>x.Id==request.Id,ct); if(item==null)return ApplicationResult.NotFound();
        item.IsActive=false;item.IsPublished=false; EndpointSupport.Audit(db,current,"COURSE_ARCHIVED","Course",item.Id);
        await db.SaveChangesAsync(ct); return ApplicationResult.NoContent();
    }
}
