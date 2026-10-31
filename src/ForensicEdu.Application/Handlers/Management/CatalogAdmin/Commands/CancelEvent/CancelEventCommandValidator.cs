using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CancelEvent;

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
