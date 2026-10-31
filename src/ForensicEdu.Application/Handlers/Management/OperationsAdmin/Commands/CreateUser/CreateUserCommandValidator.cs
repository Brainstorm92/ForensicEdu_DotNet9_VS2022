using FluentValidation;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.CreateUser;
public sealed class CreateUserCommandValidator:AbstractValidator<CreateUserCommand>
{
 public CreateUserCommandValidator(){
  RuleFor(x=>x.Input).NotNull();When(x=>x.Input!=null,()=>{
   RuleFor(x=>x.Input.Nombres).NotEmpty().MaximumLength(100);RuleFor(x=>x.Input.Apellidos).NotEmpty().MaximumLength(100);
   RuleFor(x=>x.Input.Email).NotEmpty().EmailAddress().MaximumLength(200);
   RuleFor(x=>x.Input.Password).NotEmpty().MinimumLength(12).Must(p=>p!=null&&System.Text.Encoding.UTF8.GetByteCount(p)<=72);
   RuleFor(x=>x.Input.Role).IsInEnum();RuleFor(x=>x.Input.ValidDays).InclusiveBetween(0,3650);
   RuleFor(x=>x.Input.ValidDays).GreaterThan(0).When(x=>x.Input.Role==UserRole.ESTUDIANTE);
   RuleFor(x=>x.Input.Dni).NotNull().MaximumLength(20);RuleFor(x=>x.Input.Phone).NotNull().MaximumLength(30);
  });
 }
}
