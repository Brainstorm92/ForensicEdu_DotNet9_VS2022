using System.Security.Cryptography;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;
namespace ForensicEdu.Infrastructure.Services;
public sealed class EmailConfirmationTokens(IDataProtectionProvider provider):IEmailConfirmationTokens
{
 private readonly ITimeLimitedDataProtector protector=provider.CreateProtector("ForensicEdu.EmailConfirmation.v1").ToTimeLimitedDataProtector();
 private record Payload(string UserId,string Email,string Stamp);
 public string Create(string userId,string email,string stamp)=>protector.Protect(JsonSerializer.Serialize(new Payload(userId,email,stamp)),TimeSpan.FromHours(24));
 public (string UserId,string Email,string Stamp)? Read(string token){try{var p=JsonSerializer.Deserialize<Payload>(protector.Unprotect(token));return p==null?null:(p.UserId,p.Email,p.Stamp);}catch(Exception e) when(e is CryptographicException or JsonException or FormatException or ArgumentException){return null;}}
}
