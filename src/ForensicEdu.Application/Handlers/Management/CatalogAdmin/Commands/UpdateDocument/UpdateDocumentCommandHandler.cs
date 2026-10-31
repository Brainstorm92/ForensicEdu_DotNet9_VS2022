using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateDocument;

public sealed class UpdateDocumentCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateDocumentCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateDocumentCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        var module=course?.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);if(module==null)return ApplicationResult.NotFound();
        var input=request.Input;
        var item=module.Documents.FirstOrDefault(x=>x.Id==request.DocumentId);if(item==null)return ApplicationResult.NotFound();
        if(PrivateDocuments.Key(item)!=null)return ApplicationResult.Invalid(new{message="Para reemplazar un material privado, quítalo y sube el archivo nuevo."});
        item.Title=input.Title.Trim();item.StorageUrl=input.StorageUrl;item.FileType=input.FileType;item.FileSize=input.FileSize;item.RequireWatermark=input.RequireWatermark;
        EndpointSupport.Audit(db,current,"DOCUMENT_SAVED","CourseDocument",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}
