using System.IdentityModel.Tokens.Jwt;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Contracts.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
namespace ForensicEdu.Infrastructure.Authentication;
public sealed class GoogleIdentityValidator:IGoogleIdentityValidator
{
 private readonly GoogleSignInOptions options;
 private readonly IConfigurationManager<OpenIdConnectConfiguration> metadata;
 public GoogleIdentityValidator(IOptions<GoogleSignInOptions> options,IConfigurationManager<OpenIdConnectConfiguration>? metadata=null){
  this.options=options.Value;this.metadata=metadata??new ConfigurationManager<OpenIdConnectConfiguration>("https://accounts.google.com/.well-known/openid-configuration",new OpenIdConnectConfigurationRetriever());
 }
 public bool Enabled=>options.Enabled&&!string.IsNullOrWhiteSpace(options.ClientId);
 public async Task<GoogleIdentity?> ValidateAsync(string credential,string nonce,CancellationToken ct){
  if(!Enabled||string.IsNullOrWhiteSpace(credential)||credential.Length>12000||string.IsNullOrWhiteSpace(nonce)||nonce.Length>200)return null;
  var handler=new JwtSecurityTokenHandler{MapInboundClaims=false};if(!handler.CanReadToken(credential))return null;
  for(var attempt=0;;attempt++){
   var config=await metadata.GetConfigurationAsync(ct);
   try{
    var principal=handler.ValidateToken(credential,new TokenValidationParameters{
     ValidateIssuer=true,ValidIssuers=["https://accounts.google.com","accounts.google.com"],ValidateAudience=true,ValidAudience=options.ClientId,
     ValidateIssuerSigningKey=true,IssuerSigningKeys=config.SigningKeys,RequireSignedTokens=true,RequireExpirationTime=true,ValidateLifetime=true,
     ClockSkew=TimeSpan.FromSeconds(30),ValidAlgorithms=[SecurityAlgorithms.RsaSha256]},out _);
    var subject=principal.FindFirst("sub")?.Value;var email=principal.FindFirst("email")?.Value;
    if(string.IsNullOrWhiteSpace(subject)||subject.Length>255||string.IsNullOrWhiteSpace(email)||email.Length>200||!System.Net.Mail.MailAddress.TryCreate(email,out var address)||address.Address!=email||
      !string.Equals(principal.FindFirst("email_verified")?.Value,"true",StringComparison.OrdinalIgnoreCase)||principal.FindFirst("nonce")?.Value!=nonce)return null;
    return new GoogleIdentity(subject,email.Trim().ToLowerInvariant(),principal.FindFirst("given_name")?.Value??"",principal.FindFirst("family_name")?.Value??"");
   }catch(SecurityTokenSignatureKeyNotFoundException) when(attempt==0){metadata.RequestRefresh();}
   catch(SecurityTokenException){return null;}
  }
 }
}
