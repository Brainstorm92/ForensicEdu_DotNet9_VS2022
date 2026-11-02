using MediatR;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Handlers.Auth.Commands.GoogleSignIn;
public sealed class GoogleSignInCommandHandler(IApplicationDbContext db,IGoogleIdentityValidator google,ITokenService tokens,IPasswordService passwords,IPersistenceErrors errors):IRequestHandler<GoogleSignInCommand,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(GoogleSignInCommand request,CancellationToken ct){
  if(!google.Enabled)return ApplicationResult.Unavailable(new{message="El acceso con Google no está disponible por ahora. Puedes usar tu correo y contraseña."});
  GoogleIdentity? identity;
  try{identity=await google.ValidateAsync(request.Input.Credential,request.Nonce,ct);}catch(Exception e) when(e is not OperationCanceledException){return ApplicationResult.Unavailable(new{message="No pudimos verificar tu cuenta Google. Inténtalo nuevamente."});}
  if(identity==null)return ApplicationResult.Unauthorized(new{message="No pudimos verificar tu cuenta Google. Vuelve a intentarlo."});
  var user=await db.Users.SingleOrDefaultAsync(x=>x.GoogleSubject==identity.Subject,ct);
  if(user!=null){if(Blocked(user))return Denied();return ApplicationResult.Success(new GoogleAuthResponse(Token:tokens.Create(user)));}
  user=await db.Users.SingleOrDefaultAsync(x=>x.Email.ToLower()==identity.Email,ct);
  if(user!=null){
   if(Blocked(user))return Denied();
   if(user.GoogleSubject!=null)return ApplicationResult.Conflict(new{message="Esta cuenta ya está vinculada a otra identidad de Google."});
   if(string.IsNullOrEmpty(request.Input.LinkPassword))return ApplicationResult.Success(new GoogleAuthResponse(RequiresLinkPassword:true,Email:identity.Email,Message:"Ya tienes una cuenta. Confirma tu contraseña para vincular Google."));
   if(!passwords.Verify(request.Input.LinkPassword,user.PasswordHash))return ApplicationResult.Unauthorized(new{message="La contraseña de tu cuenta es incorrecta."});
   user.GoogleSubject=identity.Subject;user.EmailConfirmedAt=DateTime.UtcNow;user.EmailConfirmationStamp=Guid.NewGuid().ToString();db.AuditLogs.Add(new AuditLog{UserId=user.Id,Action="GOOGLE_LINKED",EntityType="User",EntityId=user.Id,Details="Vinculación mediante identidad Google verificada y contraseña local."});
  }else{
   if(request.Input.Profile is not {} p)return ApplicationResult.Success(new GoogleAuthResponse(RequiresRegistration:true,Email:identity.Email,Nombres:identity.GivenName,Apellidos:identity.FamilyName,Message:"Completa tus datos para crear tu cuenta."));
   var document=p.Dni.Trim().ToUpperInvariant();if(await db.Users.AnyAsync(x=>x.Dni==document,ct))return ApplicationResult.Conflict(new{message="Ya existe una cuenta con este documento. Inicia sesión con ella o contacta a soporte."});
   user=new User{BirthDate=p.BirthDate,EmailConfirmedAt=DateTime.UtcNow,GoogleSubject=identity.Subject,Email=identity.Email,Nombres=p.Nombres.Trim(),Apellidos=p.Apellidos.Trim(),FullName=$"{p.Nombres.Trim()} {p.Apellidos.Trim()}",Dni=document,NumeroDocumento=document,Phone=p.Phone.Trim(),Telefono=p.Phone.Trim(),TipoDocumento=p.TipoDocumento,PaisResidencia=p.PaisResidencia.Trim(),NivelEducativo=p.NivelEducativo,InstitucionEducativa=p.InstitucionEducativa.Trim(),Profesion=p.Profesion.Trim(),PasswordHash="",Role=UserRole.ESTUDIANTE,Status=AccountStatus.REGISTRADA,AccountStatus=AccountStatus.REGISTRADA,ValidUntil=DateTime.UtcNow.AddDays(30),HasApprovedPurchase=false};
   db.Users.Add(user);db.AuditLogs.Add(new AuditLog{UserId=user.Id,Action="GOOGLE_REGISTERED",EntityType="User",EntityId=user.Id,Details="Alta con Google; aceptación de términos y consentimiento de tratamiento de datos personales."});
  }
  try{await db.SaveChangesAsync(ct);}catch(DbUpdateException e) when(errors.IsUniqueViolation(e)){return ApplicationResult.Conflict(new{message="La cuenta ya fue registrada o vinculada. Vuelve a iniciar sesión."});}
  return ApplicationResult.Success(new GoogleAuthResponse(Token:tokens.Create(user)));
 }
 private static bool Blocked(User user)=>user.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION or AccountStatus.VENCIDA || user.ValidUntil<=DateTime.UtcNow;
 private static ApplicationResult Denied()=>ApplicationResult.Forbidden(new{message="Tu cuenta no tiene acceso vigente. Contacta a soporte."});
}
