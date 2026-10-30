using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteModule;

public sealed class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
    }
}
