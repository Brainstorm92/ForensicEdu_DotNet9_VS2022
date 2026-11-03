using System.Text.Json;
using System.Security.Cryptography;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;
namespace ForensicEdu.Infrastructure.Services;
public sealed class VideoUploadTokens(IDataProtectionProvider protection):IVideoUploadTokens
{
 private readonly IDataProtector protector=protection.CreateProtector("ForensicEdu.PrivateVideoUpload.v1");
 public string Create(VideoUploadClaim claim)=>protector.Protect(JsonSerializer.Serialize(claim));
 public VideoUploadClaim? Read(string token){if(string.IsNullOrWhiteSpace(token)||token.Length>8192)return null;try{var claim=JsonSerializer.Deserialize<VideoUploadClaim>(protector.Unprotect(token));return claim?.ExpiresAt>DateTime.UtcNow?claim:null;}catch(Exception e) when(e is CryptographicException or JsonException or FormatException){return null;}}
}
