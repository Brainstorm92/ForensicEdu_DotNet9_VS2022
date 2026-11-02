using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Services;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var count = 0;
void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("OK: " + label); count++; }
async Task Reject(Func<Task> action, string label) { try { await action(); } catch (ArgumentException) { Check(true, label); return; } throw new Exception(label); }
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
using var db = new ApplicationDbContext(options);
var protection = new EphemeralDataProtectionProvider();
var passwords = new PasswordService();
var tokens = new PasswordRecoveryTokens(protection);
var actor = new Actor();
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Email:Address"] = "test@gmail.com", ["Email:Password"] = "fixture-only-password", ["Email:FrontendUrl"] = "https://academy.example/app", ["Email:Enabled"] = "true" }).Build();
if (args.Contains("--smtp-check"))
{
    var secrets = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", "ForensicEdu-Local-Development", "secrets.json");
    var live = new ConfigurationBuilder().AddJsonFile(secrets, optional:false).Build();
    var mail = new GmailPlatformEmail(db, protection, live, actor);
    try { await mail.TestAsync(CancellationToken.None); Console.WriteLine("Gmail aceptó el mensaje de prueba enviado a la misma cuenta configurada."); }
    catch (Exception e) { Console.WriteLine("Prueba SMTP fallida: " + e.GetType().Name); Environment.ExitCode = 1; }
    return;
}
var settings = new GmailPlatformEmail(db, protection, config, actor);
var initial = await settings.GetConfigurationAsync(default);
Check(initial.Enabled && initial.HasPassword, "Credenciales iniciales obtenidas desde configuración privada");
Check(!JsonSerializer.Serialize(initial).Contains("fixture-only-password"), "La API de configuración nunca expone la clave");
await settings.SaveConfigurationAsync(new(false,"test@gmail.com","Academia","https://academy.example/app",null),default);
var encrypted = (await db.SystemSocialSettings.SingleAsync()).Value;
Check(!encrypted.Contains("fixture-only-password") && !encrypted.Contains("test@gmail.com"), "Configuración y clave cifradas en PostgreSQL");
await settings.SaveConfigurationAsync(new(true,"test@gmail.com","Academia","https://academy.example/app",""),default);
Check((await settings.GetConfigurationAsync(default)).HasPassword, "Dejar clave vacía conserva el secreto");
await Reject(()=>settings.SaveConfigurationAsync(new(true,"other@gmail.com","Academia","https://academy.example",null),default),"Cambiar cuenta exige nueva clave");
await Reject(()=>settings.SaveConfigurationAsync(new(true,"test@gmail.com","Academia","http://outside.example",null),default),"Enlaces externos exigen HTTPS");
await Reject(()=>settings.SaveConfigurationAsync(new(true,"test@gmail.com","Academia","https://academy.example?token=x",null),default),"La raíz del frontend no admite parámetros ni fragmentos");
await Reject(()=>settings.SaveConfigurationAsync(new(true,"not-an-email","Academia","https://academy.example",null),default),"Correo inválido rechazado");
await settings.SaveConfigurationAsync(new(false,"test@gmail.com","Academia","http://localhost:3000",null),default);
try { await settings.SendAsync("recipient@example.invalid","test","test",default); throw new Exception("Disabled switch ignored"); } catch(InvalidOperationException) { Check(true,"Desactivar envío bloquea correos automáticos"); }
Check(!string.Join("",db.AuditLogs.Select(a=>a.Details)).Contains("fixture-only-password"),"Auditoría no registra contraseñas");
var mailFake = new FakeMail();
var user = new User { Id="student", Email="student@example.invalid", PasswordHash=passwords.Hash("OriginalPassword123!"), Status=AccountStatus.ACTIVO };
db.Users.Add(user);db.Users.Add(new User {Id="google-only",Email="google@example.invalid",PasswordHash=""});await db.SaveChangesAsync();
var recovery = new PasswordRecoveryService(db,mailFake,tokens,passwords);
await recovery.RequestAsync("nobody@example.invalid",default);
Check(mailFake.Body==null,"Correo inexistente devuelve éxito sin enviar ni revelar cuentas");
await recovery.RequestAsync("google@example.invalid",default);
Check(mailFake.Body==null,"Cuentas Google sin contraseña conservan su método de acceso");
await recovery.RequestAsync("STUDENT@example.invalid",default);
Check(mailFake.Recipient==user.Email && mailFake.Body!.Contains("/#reset-password="),"Recuperación usa enlace al frontend con token en fragmento");
var body=mailFake.Body!;
var encoded=body.Split("#reset-password=")[1].Split('\n')[0];
var token=Uri.UnescapeDataString(encoded);
Check(!body.Contains(user.PasswordHash),"Correo no contiene contraseña ni hash de contraseña");
Check(tokens.Read(token)?.UserId==user.Id,"Token cifrado válido identifica al usuario");
Check(tokens.Read(token+"tampered")==null,"Token manipulado es rechazado");
var expired = protection.CreateProtector("ForensicEdu.PasswordRecovery.v1").ToTimeLimitedDataProtector().Protect(JsonSerializer.Serialize(new {UserId=user.Id,PasswordHash=user.PasswordHash}),TimeSpan.FromSeconds(-1));
Check(!await recovery.ResetAsync(expired,"NewPassword123!",default),"Enlace caducado rechazado");
await Reject(()=>recovery.ResetAsync(token,"short",default),"Contraseña corta rechazada");
await Reject(()=>recovery.ResetAsync(token,new string('界',25),default),"Límite BCrypt de 72 bytes respetado");
Check(await recovery.ResetAsync(token,"NewPassword123!",default),"Enlace válido actualiza contraseña");
Check(passwords.Verify("NewPassword123!",user.PasswordHash)&&!passwords.Verify("OriginalPassword123!",user.PasswordHash),"Nueva contraseña funciona y anterior deja de funcionar");
Check(!await recovery.ResetAsync(token,"AnotherPassword123!",default),"El mismo enlace no puede reutilizarse");
Check(await db.AuditLogs.AnyAsync(a=>a.Action=="PASSWORD_RESET"),"Restablecimiento registrado en auditoría sin secretos");
// Two independent contexts read the same old hash: only one may save.
using var first=new ApplicationDbContext(options);using var second=new ApplicationDbContext(options);
var a=await first.Users.SingleAsync(u=>u.Id==user.Id);var b=await second.Users.SingleAsync(u=>u.Id==user.Id);
a.PasswordHash=passwords.Hash("ConcurrencyFirst123!");b.PasswordHash=passwords.Hash("ConcurrencySecond123!");await first.SaveChangesAsync();
try{await second.SaveChangesAsync();throw new Exception("Concurrent reset accepted");}catch(DbUpdateConcurrencyException){Check(true,"Cambios simultáneos no reutilizan el hash anterior");}
mailFake.Enabled=false;
try{await recovery.RequestAsync(user.Email,default);throw new Exception("Recovery ignored disabled switch");}catch(InvalidOperationException){Check(true,"Recuperación informa indisponibilidad cuando correos están desactivados");}
Console.WriteLine($"{count} comprobaciones de correo y recuperación superadas.");

sealed class Actor : ICurrentUser { public string UserId=>"test-admin";public bool IsInRole(string role)=>role=="1"; }
sealed class FakeMail : IPlatformEmail
{
    public string? Body;public string? Recipient;public bool Enabled=true;
    public Task<EmailConfiguration> GetConfigurationAsync(CancellationToken ct)=>Task.FromResult(new EmailConfiguration(Enabled,"test@gmail.com","Academia","https://academy.example/app",true));
    public Task SaveConfigurationAsync(UpdateEmailConfiguration r,CancellationToken ct)=>throw new NotSupportedException();
    public Task TestAsync(CancellationToken ct)=>throw new NotSupportedException();
    public Task SendAsync(string recipient,string subject,string body,CancellationToken ct){Recipient=recipient;Body=body;return Task.CompletedTask;}
}
