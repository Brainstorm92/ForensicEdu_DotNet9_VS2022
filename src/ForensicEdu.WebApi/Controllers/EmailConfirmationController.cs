using System.ComponentModel.DataAnnotations;
using ForensicEdu.Application.Common.Exceptions;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ForensicEdu.WebApi.Controllers;
[ApiController,Route("api/auth"),EnableRateLimiting("email-verification"),RequestSizeLimit(16384)]
public sealed class EmailConfirmationController(IEmailConfirmation confirmation,ILogger<EmailConfirmationController> logger):ControllerBase
{
 public record ResendRequest([Required,StringLength(200),EmailAddress]string Email);
 public record ConfirmRequest([Required,StringLength(4096)]string Token);
 [HttpPost("resend-confirmation")]
 public async Task<IActionResult> Resend(ResendRequest request,CancellationToken ct){
  try{await confirmation.RequestAsync(request.Email,ct);return Ok(new{message="Si el correo corresponde a una cuenta pendiente, recibirá un enlace de confirmación. Revise spam y espere al menos un minuto entre reenvíos."});}
  catch(EmailDeliveryUnavailableException e){logger.LogWarning("Confirmación de correo no disponible: {Reason}",e.Message);return StatusCode(503,new{message="El envío de confirmaciones está desactivado o no está configurado. Contacta a soporte o vuelve a intentarlo cuando se habilite."});}
  catch(ArgumentException e){return BadRequest(new{message=e.Message});}
 }
 [HttpPost("confirm-email")]
 public async Task<IActionResult> Confirm(ConfirmRequest request,CancellationToken ct)=>await confirmation.ConfirmAsync(request.Token,ct)?Ok(new{message="Correo confirmado. Ya puedes iniciar sesión."}):BadRequest(new{message="El enlace venció, ya fue utilizado o no es válido. Solicita una nueva confirmación."});
}
