using FluentValidation;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;

public sealed class ExamSessionsSaveCommandValidator : AbstractValidator<ExamSessionsSaveCommand>
{
    public ExamSessionsSaveCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Answers).NotNull();
    }
}
