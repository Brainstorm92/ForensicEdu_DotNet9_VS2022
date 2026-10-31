using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateModule;

public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new ModuleInputValidator());
    }
}
