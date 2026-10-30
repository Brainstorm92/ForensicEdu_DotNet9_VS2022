using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteAdvisory;

public sealed class DeleteAdvisoryCommandValidator : AbstractValidator<DeleteAdvisoryCommand>
{
    public DeleteAdvisoryCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
