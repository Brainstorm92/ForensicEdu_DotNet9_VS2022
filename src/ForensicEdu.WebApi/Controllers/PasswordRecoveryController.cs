using ForensicEdu.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ForensicEdu.WebApi.Controllers;

[ApiController, Route("api/auth"), EnableRateLimiting("password-recovery"), RequestSizeLimit(16384)]
public sealed class PasswordRecoveryController(IPasswordRecovery recovery, ILogger<PasswordRecoveryController> logger) : ControllerBase
{
    public record ForgotRequest([Required, StringLength(200), EmailAddress] string Email);
    public record ResetRequest([Required, StringLength(4096)] string Token, [Required, StringLength(72, MinimumLength = 12)] string Password);
    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotRequest request, CancellationToken ct)
    {
        try { await recovery.RequestAsync(request.Email, ct); return Ok(new { message = "Si el correo corresponde a una cuenta con contraseña, recibirá un enlace para recuperarla. Revise también spam." }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (EmailDeliveryUnavailableException e) { logger.LogWarning("Recuperación de correo no disponible: {Reason}", e.Message); return StatusCode(503, new { message = "La recuperación por correo no está disponible. Contacte a soporte." }); }
        catch (Exception e) when (e is System.Net.Mail.SmtpException or OperationCanceledException)
        {
            logger.LogWarning("Error de entrega en recuperación de contraseña: {Type}", e.GetType().Name);
            // Same response for known/unknown addresses, including delivery failures.
            return Ok(new { message = "Si el correo corresponde a una cuenta con contraseña, recibirá un enlace para recuperarla. Revise también spam." });
        }
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetRequest request, CancellationToken ct)
    {
        try { return await recovery.ResetAsync(request.Token, request.Password, ct) ? Ok(new { message = "Contraseña actualizada. Ya puede iniciar sesión." }) : BadRequest(new { message = "El enlace venció o ya fue utilizado. Solicite uno nuevo." }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
    }
}
