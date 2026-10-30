using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class PasswordInputValidator : AbstractValidator<PasswordInput>
{
    public PasswordInputValidator() { RuleFor(x=>x.CurrentPassword).NotEmpty().MaximumLength(72);
        RuleFor(x=>x.NewPassword).NotEmpty().MinimumLength(12).Must(p=>p != null && System.Text.Encoding.UTF8.GetByteCount(p)<=72).WithMessage("La contraseña admite hasta 72 bytes UTF-8."); }
}
