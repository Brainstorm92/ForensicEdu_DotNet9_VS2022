using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteModule;

public sealed class DeleteModuleCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<DeleteModuleCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(DeleteModuleCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        var item=course?.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);if(item==null)return ApplicationResult.NotFound();
        if(await db.Enrollments.AnyAsync(x=>x.CourseId==request.Id,ct)) return ApplicationResult.Conflict(new {message="No puede eliminar módulos de un curso con matrículas."});
        db.CourseModules.Remove(item);EndpointSupport.Audit(db,current,"MODULE_DELETED","CourseModule",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}
