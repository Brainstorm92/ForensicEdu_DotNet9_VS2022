using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteDocument;

public sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);
    }
}
