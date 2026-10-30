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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateDocument;

public sealed record UpdateDocumentCommand(string Id, string ModuleId, string DocumentId, DocumentInput Input) : IRequest<ApplicationResult>;
public sealed class UpdateDocumentCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateDocumentCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateDocumentCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var course=await db.Courses.Include(x=>x.Modules).ThenInclude(x=>x.Documents).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);
        var module=course?.Modules.FirstOrDefault(x=>x.Id==request.ModuleId);if(module==null)return ApplicationResult.NotFound();
        var input=request.Input;
        var item=module.Documents.FirstOrDefault(x=>x.Id==request.DocumentId);if(item==null)return ApplicationResult.NotFound();
        item.Title=input.Title.Trim();item.StorageUrl=input.StorageUrl;item.FileType=input.FileType;item.FileSize=input.FileSize;item.RequireWatermark=input.RequireWatermark;
        EndpointSupport.Audit(db,current,"DOCUMENT_SAVED","CourseDocument",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}

public sealed class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new DocumentInputValidator());
    }
}
