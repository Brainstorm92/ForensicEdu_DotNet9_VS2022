using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateDocument;

public sealed class CreateDocumentCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CreateDocumentCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        var module=course?.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);if(module==null)return ApplicationResult.NotFound();
        var input=request.Input;
        var item=new CourseDocument {ModuleId=module.Id};module.Documents.Add(item);db.CourseDocuments.Add(item);
        item.Title=input.Title.Trim();item.StorageUrl=input.StorageUrl;item.FileType=input.FileType;item.FileSize=input.FileSize;item.RequireWatermark=input.RequireWatermark;
        EndpointSupport.Audit(db,current,"DOCUMENT_SAVED","CourseDocument",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}
