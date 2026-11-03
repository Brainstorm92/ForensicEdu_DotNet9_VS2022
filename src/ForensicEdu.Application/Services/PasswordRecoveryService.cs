using ForensicEdu.Application.Common.Exceptions;
using System.Net.Mail;
using System.Text;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;
public sealed class PasswordRecoveryService(IApplicationDbContext db, IPlatformEmail email, IPasswordRecoveryTokens tokens, IPasswordService passwords) : IPasswordRecovery
{
    public async Task RequestAsync(string address, CancellationToken ct)
    {
        if (address.Length > 200 || !MailAddress.TryCreate(address, out _)) throw new ArgumentException("Escriba un correo válido.");
        var settings = await email.GetConfigurationAsync(ct);
        if (!settings.Enabled) throw new EmailDeliveryUnavailableException("EMAIL_DISABLED");
        if (!settings.HasPassword || string.IsNullOrEmpty(settings.Address) || string.IsNullOrEmpty(settings.FrontendUrl)) throw new EmailDeliveryUnavailableException("EMAIL_CONFIGURATION_INCOMPLETE");
        var normalized = address.Trim().ToLowerInvariant();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, ct);
        // Do not reveal whether an address exists or enable password login for Google-only accounts.
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || user.Status == AccountStatus.ELIMINADO || user.Status == AccountStatus.PENDIENTE_DE_ELIMINACION) return;
        var token = tokens.Create(user.Id, user.PasswordHash);
        // Fragment is not transmitted to the web server or included in HTTP access logs.
        var link = settings.FrontendUrl.TrimEnd('/') + "/#reset-password=" + Uri.EscapeDataString(token);
        await email.SendMessageAsync(user.Email, "Recupera tu contraseña — Academia Forense",
            new PlatformEmailMessage("Vuelve a tu formación.",
                "Recibimos una solicitud para restablecer la contraseña de tu cuenta. Usa el siguiente botón para crear una nueva y continuar aprendiendo.",
                "Restablecer mi contraseña", link, "Válido durante 30 minutos\nEste enlace solo puede utilizarse una vez.",
                "Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña actual seguirá siendo la misma.", "RECUPERACIÓN DE ACCESO"), ct);
    }
    public async Task<bool> ResetAsync(string token, string password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || Encoding.UTF8.GetByteCount(password) > 72) throw new ArgumentException("La contraseña debe tener al menos 12 caracteres y como máximo 72 bytes.");
        var payload = token.Length > 4096 ? null : tokens.Read(token);
        if (payload == null) return false;
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == payload.Value.UserId, ct);
        if (user == null || user.PasswordHash != payload.Value.PasswordHash || user.Status == AccountStatus.ELIMINADO || user.Status == AccountStatus.PENDIENTE_DE_ELIMINACION) return false;
        user.PasswordHash = passwords.Hash(password);
        db.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = "PASSWORD_RESET", EntityType = "User", EntityId = user.Id, Details = "Contraseña restablecida mediante enlace de correo." });
        try { await db.SaveChangesAsync(ct); return true; }
        catch (DbUpdateConcurrencyException) { return false; }
    }
}
