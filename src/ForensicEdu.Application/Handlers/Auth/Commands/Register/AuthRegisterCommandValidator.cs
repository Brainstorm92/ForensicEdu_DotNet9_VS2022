using FluentValidation;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Register;

public sealed class AuthRegisterCommandValidator : AbstractValidator<AuthRegisterCommand>
{
    public AuthRegisterCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Request.Dni).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(12).MaximumLength(72);
        RuleFor(x => x.Request.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Request.PaisResidencia).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.InstitucionEducativa).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Profesion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.TipoDocumento).Must(v => v is "DNI" or "CE" or "Pasaporte");
        RuleFor(x => x.Request.NivelEducativo).Must(v => v is "Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado");
        RuleFor(x => x.Request.BirthDate).Must(ForensicEdu.Application.Common.Validation.BirthDateRules.Valid).WithMessage("Indique su fecha de nacimiento; no puede ser una fecha futura.");
        RuleFor(x => x.Request.AcceptTerms).Equal(true);
        RuleFor(x => x.Request.AcceptPrivacy).Equal(true);
        });
    }
}
