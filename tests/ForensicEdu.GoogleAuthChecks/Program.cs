using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Application.Handlers.Auth.Commands.GoogleSignIn;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Authentication;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("OK: "+name);}
using var rsa=RSA.Create(2048);var key=new RsaSecurityKey(rsa){KeyId="fixture"};
const string audience="fixture.apps.googleusercontent.com",nonce="nonce-de-prueba";
var config=new OpenIdConnectConfiguration();config.SigningKeys.Add(key);
var validator=new GoogleIdentityValidator(Options.Create(new GoogleSignInOptions{Enabled=true,ClientId=audience}),new Metadata(config));
string Token(string aud=audience,string issuer="https://accounts.google.com",string? tokenNonce=nonce,bool verified=true,bool expired=false,SecurityKey? signer=null){
 var claims=new List<Claim>{new("sub","google-fixture"),new("email","alumno@example.invalid"),new("email_verified",verified?"true":"false",ClaimValueTypes.Boolean),new("given_name","Alumna"),new("family_name","Prueba")};
 if(tokenNonce!=null)claims.Add(new("nonce",tokenNonce));
 return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer,aud,claims,DateTime.UtcNow.AddHours(-2),expired?DateTime.UtcNow.AddHours(-1):DateTime.UtcNow.AddMinutes(5),new SigningCredentials(signer??key,SecurityAlgorithms.RsaSha256)));
}
Check((await validator.ValidateAsync(Token(),nonce,default))?.Subject=="google-fixture","Firma Google, audiencia, emisor, vigencia y nonce verificados");
Check(await validator.ValidateAsync(Token(aud:"otra-app"),nonce,default)==null,"Rechaza token para otra aplicación");
Check(await validator.ValidateAsync(Token(issuer:"https://evil.invalid"),nonce,default)==null,"Rechaza emisor diferente a Google");
Check(await validator.ValidateAsync(Token(expired:true),nonce,default)==null,"Rechaza token vencido");
Check(await validator.ValidateAsync(Token(tokenNonce:"otra-sesion"),nonce,default)==null,"Rechaza identidad de otra sesión");
Check(await validator.ValidateAsync(Token(tokenNonce:null),nonce,default)==null,"Rechaza token sin nonce");
Check(await validator.ValidateAsync(Token(verified:false),nonce,default)==null,"Rechaza correo no verificado");
using var wrong=RSA.Create(2048);Check(await validator.ValidateAsync(Token(signer:new RsaSecurityKey(wrong){KeyId="incorrecta"}),nonce,default)==null,"Rechaza firma ajena a Google");
Check(await validator.ValidateAsync("no-es-un-token",nonce,default)==null,"Rechaza credenciales malformadas");

var db=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
var google=new IdentityAdapter();var passwords=new PasswordService();var handler=new GoogleSignInCommandHandler(db,google,new Tokens(),passwords,new PersistenceErrors());
GoogleSignInCommand Command(GoogleProfileInput? profile=null,string? password=null)=>new(new GoogleSignInInput("fixture",profile,password),nonce);
var first=await handler.Handle(Command(),default);Check(first.Value is GoogleAuthResponse{RequiresRegistration:true}&&!await db.Users.AnyAsync(),"Primer acceso solicita perfil sin crear una cuenta incompleta");
var profile=new GoogleProfileInput("Alumna","Google","GTEST001","999000111","DNI","Perú","Licenciatura","Instituto","Perito",true,true,new DateOnly(1995,4,20));
var inputValidator=new GoogleSignInCommandValidator();Check(!inputValidator.Validate(Command(profile with{AcceptTerms=false})).IsValid,"Registro Google exige aceptar términos");
Check(!inputValidator.Validate(Command(profile with{AcceptPrivacy=false})).IsValid,"Registro Google exige consentimiento de datos");
var signup=await handler.Handle(Command(profile),default);var student=await db.Users.SingleAsync();Check(signup.Value is GoogleAuthResponse{Token:not null}&&student.Role==UserRole.ESTUDIANTE&&student.GoogleSubject==google.Identity.Subject&&student.PasswordHash=="","Alta Google crea estudiante con identidad propia y sin contraseña ficticia");
Check(student.BirthDate==profile.BirthDate&&student.EmailConfirmedAt.HasValue,"Google conserva nacimiento y reconoce el correo verificado por el proveedor");
Check(!inputValidator.Validate(Command(profile with{BirthDate=null})).IsValid,"Registro Google exige fecha de nacimiento");
Check(student.ValidUntil>DateTime.UtcNow.AddDays(29)&&!student.HasApprovedPurchase,"Google conserva vigencia inicial y no simula una compra");
Check((await handler.Handle(Command(),default)).Value is GoogleAuthResponse{Token:not null}&&await db.Users.CountAsync()==1,"Reingreso usa sub sin duplicar usuarios");
Check(!passwords.Verify("cualquier-clave",student.PasswordHash),"Cuenta Google no produce error al intentar acceso con contraseña");
student.Status=AccountStatus.SUSPENDIDO;await db.SaveChangesAsync();Check((await handler.Handle(Command(),default)).Outcome==ResultOutcome.Forbidden,"Google no permite entrar a cuentas suspendidas");
student.Status=AccountStatus.ACTIVO;student.ValidUntil=DateTime.UtcNow.AddDays(-1);await db.SaveChangesAsync();Check((await handler.Handle(Command(),default)).Outcome==ResultOutcome.Forbidden,"Google no renueva cuentas vencidas");

var local=new User{Id="cuenta-local",Email="local@example.invalid",FullName="Administradora",PasswordHash=passwords.Hash("ClaveLocalSegura2026#"),Role=UserRole.ADMIN_FINANCIERO,ValidUntil=null};db.Users.Add(local);await db.SaveChangesAsync();google.Identity=new GoogleIdentity("google-local",local.Email,"Admin","Prueba");
Check((await handler.Handle(Command(),default)).Value is GoogleAuthResponse{RequiresLinkPassword:true}&&local.GoogleSubject==null,"Coincidir por correo no vincula ni toma una cuenta existente");
Check((await handler.Handle(Command(password:"incorrecta"),default)).Outcome==ResultOutcome.Unauthorized&&local.GoogleSubject==null,"Vinculación exige la contraseña local correcta");
Check((await handler.Handle(Command(password:"ClaveLocalSegura2026#"),default)).Value is GoogleAuthResponse{Token:not null}&&local.GoogleSubject=="google-local"&&local.Role==UserRole.ADMIN_FINANCIERO,"Vinculación explícita conserva identidad, rol y contraseña");
google.Identity=new GoogleIdentity("otra-identidad",local.Email,"Otra","Prueba");Check((await handler.Handle(Command(password:"ClaveLocalSegura2026#"),default)).Outcome==ResultOutcome.Conflict,"Una identidad Google no reemplaza la vinculación de otra");
google.Identity=new GoogleIdentity("nuevo-google","nuevo@example.invalid","Nuevo","Alumno");Check((await handler.Handle(Command(profile),default)).Outcome==ResultOutcome.Conflict,"Google no crea una cuenta con documento ya registrado");
google.Enabled=false;Check((await handler.Handle(Command(),default)).Outcome==ResultOutcome.Unavailable,"Configuración ausente no habilita Google ficticiamente");
Console.WriteLine("PASS: criptografía, vinculación, perfiles y restricciones Google verificadas con claves sintéticas; no se autenticó una cuenta Google real.");

sealed class Metadata(OpenIdConnectConfiguration config):IConfigurationManager<OpenIdConnectConfiguration>{public Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken ct)=>Task.FromResult(config);public void RequestRefresh(){}}
sealed class IdentityAdapter:IGoogleIdentityValidator{public bool Enabled{get;set;}=true;public GoogleIdentity Identity{get;set;}=new("google-new","alumna@example.invalid","Alumna","Google");public Task<GoogleIdentity?> ValidateAsync(string credential,string nonce,CancellationToken ct)=>Task.FromResult<GoogleIdentity?>(Identity);}
sealed class Tokens:ITokenService{public string Create(User user)=>"session-"+user.Id;}
