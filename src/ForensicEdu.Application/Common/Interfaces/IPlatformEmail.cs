namespace ForensicEdu.Application.Common.Interfaces;

public record EmailConfiguration(bool Enabled, string Address, string SenderName, string FrontendUrl, bool HasPassword);
public record UpdateEmailConfiguration(bool Enabled, string Address, string SenderName, string FrontendUrl, string? Password);
public interface IPlatformEmail
{
    Task<EmailConfiguration> GetConfigurationAsync(CancellationToken ct);
    Task SaveConfigurationAsync(UpdateEmailConfiguration request, CancellationToken ct);
    Task TestAsync(CancellationToken ct);
    Task SendAsync(string recipient, string subject, string body, CancellationToken ct);
    Task SendMessageAsync(string recipient, string subject, PlatformEmailMessage message, CancellationToken ct)
        => SendAsync(recipient, subject, message.ToPlainText(), ct);
}
public interface IPasswordRecovery
{
    Task RequestAsync(string email, CancellationToken ct);
    Task<bool> ResetAsync(string token, string password, CancellationToken ct);
}
public interface IPasswordRecoveryTokens
{
    string Create(string userId, string passwordHash);
    (string UserId, string PasswordHash)? Read(string token);
}
