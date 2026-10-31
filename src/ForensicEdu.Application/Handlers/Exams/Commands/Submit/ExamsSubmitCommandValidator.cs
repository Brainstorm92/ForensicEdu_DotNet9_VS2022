using FluentValidation;
namespace ForensicEdu.Application.Handlers.Exams.Commands.Submit;

public sealed class ExamsSubmitCommandValidator : AbstractValidator<ExamsSubmitCommand>
{
    public ExamsSubmitCommandValidator()
    {
        RuleFor(x => x.Submission).NotNull();
        When(x => x.Submission != null, () => {
        RuleFor(x => x.Submission.EnrollmentId).NotEmpty();
        RuleFor(x => x.Submission.UserId).NotEmpty();
        RuleFor(x => x.Submission.SelectedAnswers).NotNull();
        });
    }
}
