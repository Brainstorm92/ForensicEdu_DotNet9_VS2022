using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<DeleteDocumentCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        var module=course?.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);var item=module?.Documents.FirstOrDefault(x=>x.Id==request.DocumentId);
        if(item==null)return ApplicationResult.NotFound();db.CourseDocuments.Remove(item);
        EndpointSupport.Audit(db,current,"DOCUMENT_DELETED","CourseDocument",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}
