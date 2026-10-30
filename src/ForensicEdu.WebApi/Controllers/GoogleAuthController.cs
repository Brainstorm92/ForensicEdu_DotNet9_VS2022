using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Application.Handlers.Auth.Commands.GoogleSignIn;
using ForensicEdu.Infrastructure.Authentication;
using ForensicEdu.WebApi.Common;
namespace ForensicEdu.WebApi.Controllers;
[ApiController]
[Route("api/auth/google")]
public sealed class GoogleAuthController(ISender sender,IOptions<GoogleSignInOptions> options,IWebHostEnvironment environment):ControllerBase
{
 private const string Cookie="forensicedu.google.nonce";
 private CookieOptions CookieOptions()=>new(){HttpOnly=true,Secure=!environment.IsDevelopment()||Request.IsHttps,SameSite=SameSiteMode.Lax,Path="/api/auth/google",MaxAge=TimeSpan.FromMinutes(10),IsEssential=true};
 [HttpGet("config")]
 public IActionResult Config(){
  Response.Headers.CacheControl="no-store";var enabled=options.Value.Enabled&&!string.IsNullOrWhiteSpace(options.Value.ClientId);
  if(!enabled)return Ok(new{enabled=false,clientId="",nonce=""});
  var nonce=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));Response.Cookies.Append(Cookie,nonce,CookieOptions());return Ok(new{enabled=true,clientId=options.Value.ClientId,nonce});
 }
 [HttpPost]
 [RequestSizeLimit(24000)]
 public async Task<IActionResult> SignIn([FromBody]GoogleSignInInput input){
  Response.Headers.CacheControl="no-store";
  if(!Request.Cookies.TryGetValue(Cookie,out var nonce)||string.IsNullOrEmpty(nonce))return Unauthorized(new{message="Tu acceso con Google ha caducado. Cierra esta ventana y vuelve a abrirla."});
  var result=await sender.Send(new GoogleSignInCommand(input,nonce),HttpContext.RequestAborted);
  if(result.Value is GoogleAuthResponse{Token:not null}){var cookie=CookieOptions();cookie.MaxAge=null;Response.Cookies.Delete(Cookie,cookie);}
  return result.ToActionResult();
 }
}
