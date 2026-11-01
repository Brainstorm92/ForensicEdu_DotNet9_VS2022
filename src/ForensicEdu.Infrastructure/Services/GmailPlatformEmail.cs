using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ForensicEdu.Infrastructure.Services;

// Passwords are never returned to clients. The complete database payload is protected.
public sealed class GmailPlatformEmail(IApplicationDbContext db, IDataProtectionProvider protection, IConfiguration configuration, ICurrentUser current) : IPlatformEmail
{
    private const string Key = "PlatformEmail.Protected.v1";
    private readonly IDataProtector protector = protection.CreateProtector("ForensicEdu.EmailConfiguration.v1");
    private sealed record Stored(bool Enabled, string Address, string SenderName, string FrontendUrl, string Password);
    private async Task<Stored> Load(CancellationToken ct)
    {
        var saved = await db.SystemSocialSettings.AsNoTracking().Where(s => s.Key == Key).Select(s => s.Value).SingleOrDefaultAsync(ct);
        if (saved != null) return JsonSerializer.Deserialize<Stored>(protector.Unprotect(saved)) ?? throw new InvalidOperationException("Configuración de correo inválida.");
        return new(configuration.GetValue<bool>("Email:Enabled"), configuration["Email:Address"] ?? "", configuration["Email:SenderName"] ?? "Academia Forense", configuration["Email:FrontendUrl"] ?? "", configuration["Email:Password"] ?? "");
    }
    public async Task<EmailConfiguration> GetConfigurationAsync(CancellationToken ct)
    {
        var s = await Load(ct);
        return new(s.Enabled, s.Address, s.SenderName, s.FrontendUrl, !string.IsNullOrEmpty(s.Password));
    }
    public async Task SaveConfigurationAsync(UpdateEmailConfiguration request, CancellationToken ct)
    {
        if (!MailAddress.TryCreate(request.Address, out var address) || address.Address != request.Address.Trim() || !address.Host.Equals("gmail.com", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Indique una dirección de Gmail válida.");
        if (string.IsNullOrWhiteSpace(request.SenderName) || request.SenderName.Length > 100 || request.SenderName.Any(char.IsControl))
            throw new ArgumentException("Indique un nombre de remitente de hasta 100 caracteres.");
        if (!Uri.TryCreate(request.FrontendUrl, UriKind.Absolute, out var url) || (url.Scheme != "https" && !(url.Scheme == "http" && url.IsLoopback)) || !string.IsNullOrEmpty(url.UserInfo) || !string.IsNullOrEmpty(url.Query) || !string.IsNullOrEmpty(url.Fragment))
            throw new ArgumentException("Use la URL HTTPS del sitio web; HTTP solo se admite en localhost.");
        var previous = await Load(ct);
        var password = string.IsNullOrWhiteSpace(request.Password) ? previous.Password : request.Password.Replace(" ", "");
        if (request.Enabled && string.IsNullOrEmpty(password)) throw new ArgumentException("Configure la clave de aplicación de Gmail.");
        if (password.Length > 256) throw new ArgumentException("La clave es demasiado larga.");
        if (!previous.Address.Equals(address.Address, StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.Password) && !string.IsNullOrEmpty(previous.Password))
            throw new ArgumentException("Al cambiar la cuenta, indique su nueva clave de aplicación.");
        var setting = await db.SystemSocialSettings.SingleOrDefaultAsync(s => s.Key == Key, ct);
        if (setting == null) { setting = new SystemSocialSetting { Key = Key }; db.SystemSocialSettings.Add(setting); }
        setting.Value = protector.Protect(JsonSerializer.Serialize(new Stored(request.Enabled, address.Address, request.SenderName.Trim(), url.AbsoluteUri.TrimEnd('/'), password)));
        setting.UpdatedAt = DateTime.UtcNow;
        // Saving corrected credentials or re-enabling delivery releases exhausted retries.
        if (request.Enabled)
        {
            var failed = await db.StudentNotifications.Where(n => n.EmailStatus == "FAILED").ToListAsync(ct);
            foreach (var notice in failed)
            {
                notice.EmailStatus = "PENDING"; notice.EmailAttempts = 0;
                notice.NextEmailAttemptAt = null; notice.Version = Guid.NewGuid().ToString();
            }
        }
        db.AuditLogs.Add(new AuditLog { UserId = current.UserId, Action = "EMAIL_CONFIGURATION_UPDATED", EntityType = "EmailConfiguration", Details = request.Enabled ? "Envío habilitado" : "Envío deshabilitado" });
        await db.SaveChangesAsync(ct);
    }
    public async Task TestAsync(CancellationToken ct)
    {
        var s = await Load(ct);
        // Explicit administrator diagnostic works even when automatic sending is disabled.
        await Deliver(s, s.Address, "Prueba de correo — Academia Forense", "La conexión SMTP y las credenciales de correo funcionan correctamente. Este es un mensaje de prueba del panel administrativo.", ct);
    }
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken ct)
    {
        var s = await Load(ct);
        if (!s.Enabled) throw new InvalidOperationException("El envío automático de correo está deshabilitado.");
        await Deliver(s, recipient, subject, body, ct);
    }
    public async Task SendMessageAsync(string recipient, string subject, PlatformEmailMessage message, CancellationToken ct)
    {
        var s = await Load(ct);
        if (!s.Enabled) throw new InvalidOperationException("El envío automático de correo está deshabilitado.");
        await Deliver(s, recipient, subject, message, ct);
    }
    private static Task Deliver(Stored s, string recipient, string subject, string body, CancellationToken ct)
        => Deliver(s, recipient, subject, new PlatformEmailMessage(subject, body), ct);
    private static async Task Deliver(Stored s, string recipient, string subject, PlatformEmailMessage content, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(s.Password) || !MailAddress.TryCreate(s.Address, out _)) throw new InvalidOperationException("Configure la cuenta y clave de aplicación de Gmail.");
        using var message = new MailMessage { From = new MailAddress(s.Address, s.SenderName), Subject = subject, Body = content.ToPlainText(), IsBodyHtml = false, BodyEncoding = Encoding.UTF8, SubjectEncoding = Encoding.UTF8 };
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(PlatformEmailTemplate.Render(content), Encoding.UTF8, MediaTypeNames.Text.Html));
        message.To.Add(new MailAddress(recipient));
        using var smtp = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, UseDefaultCredentials = false, Credentials = new NetworkCredential(s.Address, s.Password), Timeout = 20000 };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(25));
        await smtp.SendMailAsync(message, timeout.Token);
    }
}
