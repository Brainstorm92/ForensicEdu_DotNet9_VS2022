using FluentValidation;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.CreateTicket;

public sealed class StudentServicesCreateTicketCommandValidator : AbstractValidator<StudentServicesCreateTicketCommand>
{
    public StudentServicesCreateTicketCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Message).NotEmpty().MaximumLength(10000);
        });
    }
}
