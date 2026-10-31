using FluentValidation;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Reply;

public sealed class StudentServicesReplyCommandValidator : AbstractValidator<StudentServicesReplyCommand>
{
    public StudentServicesReplyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Message).NotEmpty().MaximumLength(10000);
        });
    }
}
