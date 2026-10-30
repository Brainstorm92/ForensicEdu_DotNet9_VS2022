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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteDocument;

public sealed record DeleteDocumentCommand(string Id, string ModuleId, string DocumentId) : IRequest<ApplicationResult>;
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

public sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);
    }
}
