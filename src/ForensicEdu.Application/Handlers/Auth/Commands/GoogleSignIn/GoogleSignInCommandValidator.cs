using FluentValidation;
namespace ForensicEdu.Application.Handlers.Auth.Commands.GoogleSignIn;
public sealed class GoogleSignInCommandValidator:AbstractValidator<GoogleSignInCommand>
{
 public GoogleSignInCommandValidator(){
  RuleFor(x=>x.Input).NotNull();RuleFor(x=>x.Nonce).NotEmpty().MaximumLength(200).WithMessage("Vuelve a abrir el acceso con Google.");
  When(x=>x.Input!=null,()=>{RuleFor(x=>x.Input.Credential).NotEmpty().MaximumLength(12000);RuleFor(x=>x.Input.LinkPassword).MaximumLength(72);
   When(x=>x.Input.Profile!=null,()=>{
    RuleFor(x=>x.Input.Profile!.Nombres).NotEmpty().MaximumLength(100);RuleFor(x=>x.Input.Profile!.Apellidos).NotEmpty().MaximumLength(100);
    RuleFor(x=>x.Input.Profile!.Dni).NotEmpty().MaximumLength(20);RuleFor(x=>x.Input.Profile!.Phone).NotEmpty().MaximumLength(30);
    RuleFor(x=>x.Input.Profile!.PaisResidencia).NotEmpty().MaximumLength(100);RuleFor(x=>x.Input.Profile!.InstitucionEducativa).NotEmpty().MaximumLength(300);
    RuleFor(x=>x.Input.Profile!.Profesion).NotEmpty().MaximumLength(200);RuleFor(x=>x.Input.Profile!.TipoDocumento).Must(v=>v is "DNI" or "CE" or "Pasaporte");
    RuleFor(x=>x.Input.Profile!.NivelEducativo).Must(v=>v is "Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado");
    RuleFor(x=>x.Input.Profile!.AcceptTerms).Equal(true);RuleFor(x=>x.Input.Profile!.AcceptPrivacy).Equal(true);
   });
  });
 }
}
