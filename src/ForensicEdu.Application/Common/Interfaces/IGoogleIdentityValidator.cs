using ForensicEdu.Application.Contracts.Auth;
namespace ForensicEdu.Application.Common.Interfaces;
public interface IGoogleIdentityValidator
{
 bool Enabled {get;}
 Task<GoogleIdentity?> ValidateAsync(string credential,string nonce,CancellationToken ct);
}
