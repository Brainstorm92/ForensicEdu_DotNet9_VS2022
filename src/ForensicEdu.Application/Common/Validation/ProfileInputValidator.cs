using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class ProfileInputValidator : AbstractValidator<ProfileInput>
{
    public ProfileInputValidator() { RuleFor(x=>x.Nombres + " " + x.Apellidos).MaximumLength(200); RuleFor(x=>x.Nombres).NotEmpty().MaximumLength(100); RuleFor(x=>x.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Phone).NotEmpty().MaximumLength(30); RuleFor(x=>x.PaisResidencia).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.NivelEducativo).Must(v=>v is "Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado");
        RuleFor(x=>x.InstitucionEducativa).NotEmpty().MaximumLength(300); RuleFor(x=>x.Profesion).NotEmpty().MaximumLength(200); }
}
