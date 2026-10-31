using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class TicketStatusInputValidator : AbstractValidator<TicketStatusInput>
{
    public TicketStatusInputValidator() { RuleFor(x=>x.Status).IsInEnum(); }
}
