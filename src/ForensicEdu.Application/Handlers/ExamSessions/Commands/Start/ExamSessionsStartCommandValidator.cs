using FluentValidation;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;

public sealed class ExamSessionsStartCommandValidator : AbstractValidator<ExamSessionsStartCommand>
{
    public ExamSessionsStartCommandValidator()
    {
        RuleFor(x => x.EnrollmentId).NotEmpty().MaximumLength(200);
    }
}
