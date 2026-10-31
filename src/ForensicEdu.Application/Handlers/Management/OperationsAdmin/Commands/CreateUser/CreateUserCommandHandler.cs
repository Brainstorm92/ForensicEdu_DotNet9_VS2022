using MediatR;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.CreateUser;
public sealed class CreateUserCommandHandler(IApplicationDbContext db,ICurrentUser current,IPasswordService passwords):IRequestHandler<CreateUserCommand,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(CreateUserCommand request,CancellationToken ct){
  if(!EndpointSupport.HasRole(current,"1"))return ApplicationResult.Forbidden();
  var i=request.Input;var email=i.Email.Trim().ToLowerInvariant();
  if(await db.Users.AnyAsync(x=>x.Email.ToLower()==email,ct))return ApplicationResult.Conflict(new {message="El correo ya está registrado."});
  var u=new User{Nombres=i.Nombres.Trim(),Apellidos=i.Apellidos.Trim(),FullName=$"{i.Nombres.Trim()} {i.Apellidos.Trim()}",Email=email,PasswordHash=passwords.Hash(i.Password),Role=i.Role,Status=AccountStatus.ACTIVO,AccountStatus=AccountStatus.ACTIVO,ValidUntil=i.ValidDays==0?null:DateTime.UtcNow.AddDays(i.ValidDays),Dni=i.Dni,NumeroDocumento=i.Dni,Phone=i.Phone,Telefono=i.Phone};
  db.Users.Add(u);EndpointSupport.Audit(db,current,"USER_CREATED","User",u.Id,i.Role.ToString());await db.SaveChangesAsync(ct);
  return ApplicationResult.Success(new{u.Id,u.FullName,u.Email,u.Role,u.ValidUntil});
 }
}
