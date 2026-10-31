using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateAdvisory;

public sealed class CreateAdvisoryCommandValidator : AbstractValidator<CreateAdvisoryCommand>
{
    public CreateAdvisoryCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new AdvisoryInputValidator());
    }
}
