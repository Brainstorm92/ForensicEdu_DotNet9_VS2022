using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateModule;

public sealed class UpdateModuleCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateModuleCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateModuleCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        if(course==null)return ApplicationResult.NotFound();
        var input=request.Input;
        var item=course.Modules.FirstOrDefault(x=>x.Id==request.ModuleId); if(item==null)return ApplicationResult.NotFound();
        if(course.Modules.Any(x=>x.Id!=item.Id && x.OrderIndex==input.OrderIndex))return ApplicationResult.Conflict(new {message="Ese orden ya pertenece a otro módulo."});
        item.Title=input.Title.Trim();item.OrderIndex=input.OrderIndex;item.Duration=input.Duration;item.VideoStreamingUrl=input.VideoStreamingUrl;
        EndpointSupport.Audit(db,current,"MODULE_SAVED","CourseModule",item.Id);await db.SaveChangesAsync(ct);
        return ApplicationResult.Success(new {item.Id,item.Title,item.OrderIndex});
    }
}
