using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetTicketStatus;

public sealed class SetTicketStatusCommandValidator : AbstractValidator<SetTicketStatusCommand>
{
    public SetTicketStatusCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new TicketStatusInputValidator());
    }
}
