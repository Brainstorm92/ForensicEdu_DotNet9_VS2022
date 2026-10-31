using FluentValidation;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Login;

public sealed class AuthLoginCommandValidator : AbstractValidator<AuthLoginCommand>
{
    public AuthLoginCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Email).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Password).NotEmpty().MaximumLength(72);
        });
    }
}
