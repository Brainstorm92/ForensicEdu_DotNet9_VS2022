using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace ForensicEdu.WebApi.Controllers;

[ApiController, Route("api/admin/email"), Authorize(Roles = "1"), RequestSizeLimit(16384)]
public sealed class EmailSettingsController(IPlatformEmail email, ILogger<EmailSettingsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) { Response.Headers.CacheControl = "no-store"; return Ok(await email.GetConfigurationAsync(ct)); }
    [HttpPut]
    public async Task<IActionResult> Save(UpdateEmailConfiguration request, CancellationToken ct)
    {
        try { await email.SaveConfigurationAsync(request, ct); return NoContent(); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
    }
    [HttpPost("test"), EnableRateLimiting("email-test")]
    public async Task<IActionResult> Test(CancellationToken ct)
    {
        try { await email.TestAsync(ct); return Ok(new { message = "Gmail aceptó el mensaje de prueba. Revise la bandeja de la cuenta configurada y spam." }); }
        catch (Exception e) when (e is System.Net.Mail.SmtpException or InvalidOperationException or OperationCanceledException)
        { logger.LogWarning("Prueba SMTP fallida: {Type}", e.GetType().Name); return StatusCode(503, new { message = "No se pudo enviar. Revise la cuenta, la clave de aplicación de Gmail y la salida TCP al puerto 587 del servidor." }); }
    }
}
