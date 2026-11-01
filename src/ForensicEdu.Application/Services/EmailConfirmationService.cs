using System.Net.Mail;
using ForensicEdu.Application.Common.Exceptions;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace ForensicEdu.Application.Services;
public sealed class EmailConfirmationService(IApplicationDbContext db,IPlatformEmail email,IEmailConfirmationTokens tokens,ILogger<EmailConfirmationService> logger):IEmailConfirmation
{
 public async Task<ConfirmationDelivery> SendInitialAsync(User user,CancellationToken ct){
  try{return await Send(user,await email.GetConfigurationAsync(ct),ct);}
  catch(Exception e) when(e is not OperationCanceledException||!ct.IsCancellationRequested){logger.LogWarning("Confirmación de correo pendiente; fallo de tipo {Type}",e.GetType().Name);return ConfirmationDelivery.Failed;}
 }
 private async Task<ConfirmationDelivery> Send(User user,EmailConfiguration settings,CancellationToken ct){
  if(!settings.Enabled)return ConfirmationDelivery.Disabled;
  if(!settings.HasPassword||string.IsNullOrWhiteSpace(settings.Address)||string.IsNullOrWhiteSpace(settings.FrontendUrl))return ConfirmationDelivery.Unavailable;
  if(user.EmailConfirmedAt.HasValue||!user.EmailConfirmationRequired)return ConfirmationDelivery.Throttled;
  if(user.EmailConfirmationSentAt>DateTime.UtcNow.AddMinutes(-1))return ConfirmationDelivery.Throttled;
  // Claim before SMTP. A concurrent resend loses the version check and sends nothing.
  user.EmailConfirmationSentAt=DateTime.UtcNow;
  try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){return ConfirmationDelivery.Throttled;}
  var link=settings.FrontendUrl.TrimEnd('/')+"/#confirm-email="+Uri.EscapeDataString(tokens.Create(user.Id,user.Email,user.EmailConfirmationStamp));
  await email.SendMessageAsync(user.Email,"Confirma tu correo — Academia Forense",new PlatformEmailMessage("Tu formación empieza aquí.","Tu cuenta está creada. Confirma tu correo para activar el acceso a la academia.","Confirmar mi correo",link,"Válido durante 24 horas\nEste enlace solo puede utilizarse una vez.","Si no solicitaste esta cuenta, puedes ignorar este correo.","CONFIRMACIÓN DE CUENTA"),ct);
  return ConfirmationDelivery.Sent;
 }
 public async Task RequestAsync(string address,CancellationToken ct){
  if(address.Length>200||!MailAddress.TryCreate(address,out _))throw new ArgumentException("Escriba un correo válido.");
  var settings=await email.GetConfigurationAsync(ct);
  if(!settings.Enabled)throw new EmailDeliveryUnavailableException("EMAIL_DISABLED");
  if(!settings.HasPassword||string.IsNullOrWhiteSpace(settings.Address)||string.IsNullOrWhiteSpace(settings.FrontendUrl))throw new EmailDeliveryUnavailableException("EMAIL_CONFIGURATION_INCOMPLETE");
  var normalized=address.Trim().ToLowerInvariant();var user=await db.Users.SingleOrDefaultAsync(u=>u.Email.ToLower()==normalized,ct);
  if(user==null||user.Status is AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION)return;
  try{await Send(user,settings,ct);}catch(Exception e) when(e is not OperationCanceledException||!ct.IsCancellationRequested){logger.LogWarning("Reenvío de confirmación pendiente; fallo de tipo {Type}",e.GetType().Name);}
 }
 public async Task<bool> ConfirmAsync(string token,CancellationToken ct){
  if(string.IsNullOrWhiteSpace(token)||token.Length>4096)return false;
  var payload=tokens.Read(token);if(payload==null)return false;
  var user=await db.Users.SingleOrDefaultAsync(u=>u.Id==payload.Value.UserId,ct);
  if(user==null||user.Email!=payload.Value.Email||user.EmailConfirmationStamp!=payload.Value.Stamp||user.EmailConfirmedAt.HasValue||!user.EmailConfirmationRequired||user.Status is AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION)return false;
  user.EmailConfirmedAt=DateTime.UtcNow;user.EmailConfirmationStamp=Guid.NewGuid().ToString();
  db.AuditLogs.Add(new(){UserId=user.Id,Action="EMAIL_CONFIRMED",EntityType="User",EntityId=user.Id,Details="Correo confirmado mediante enlace de un solo uso."});
  try{await db.SaveChangesAsync(ct);return true;}catch(DbUpdateConcurrencyException){return false;}
 }
}
