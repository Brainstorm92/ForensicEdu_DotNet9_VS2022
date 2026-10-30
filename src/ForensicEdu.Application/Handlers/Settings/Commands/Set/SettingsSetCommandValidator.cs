using FluentValidation;
namespace ForensicEdu.Application.Handlers.Settings.Commands.Set;

public sealed class SettingsSetCommandValidator : AbstractValidator<SettingsSetCommand>
{
    public SettingsSetCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Value).NotNull().MaximumLength(2048);
        });
    }
}
