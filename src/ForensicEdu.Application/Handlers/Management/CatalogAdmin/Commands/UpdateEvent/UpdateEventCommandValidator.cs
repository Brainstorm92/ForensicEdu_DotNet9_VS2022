using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateEvent;

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new EventInputValidator());
    }
}
