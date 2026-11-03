using System.Security.Cryptography;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace ForensicEdu.Infrastructure.Services;
public sealed class PasswordRecoveryTokens(IDataProtectionProvider protection) : IPasswordRecoveryTokens
{
    private readonly ITimeLimitedDataProtector protector = protection.CreateProtector("ForensicEdu.PasswordRecovery.v1").ToTimeLimitedDataProtector();
    public string Create(string userId, string passwordHash) => protector.Protect(JsonSerializer.Serialize(new Payload(userId, passwordHash)), TimeSpan.FromMinutes(30));
    public (string UserId, string PasswordHash)? Read(string token)
    {
        try { var value = JsonSerializer.Deserialize<Payload>(protector.Unprotect(token)); return value == null ? null : (value.UserId, value.PasswordHash); }
        catch (Exception e) when (e is CryptographicException or JsonException or FormatException or ArgumentException) { return null; }
    }
    private record Payload(string UserId, string PasswordHash);
}
