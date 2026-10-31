using FluentValidation;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Complete;

public sealed class StudentServicesCompleteCommandValidator : AbstractValidator<StudentServicesCompleteCommand>
{
    public StudentServicesCompleteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModuleId).NotEmpty().MaximumLength(200);
    }
}
