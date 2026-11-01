using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Contracts.Auth;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
static class RegistrationEmailScenario
{
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens){
  var count=0;void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  var mail=services.GetRequiredService<MailCapture>();
  var input=new RegisterRequest("Alumna Registro","signup@example.invalid","DOC-REG-1","999888777","OriginalPassword123!","Alumna","Registro","DNI","Perú","Licenciatura","Instituto","Perito",true,true,new DateOnly(1996,2,29));
  async Task<HttpResponseMessage> Register(RegisterRequest request)=>await http.PostAsJsonAsync("/api/auth/register",request);
  async Task<HttpResponseMessage> Login(string email,string password="OriginalPassword123!")=>await http.PostAsJsonAsync("/api/auth/login",new LoginRequest(email,password));
  async Task<HttpResponseMessage> Resend(string email)=>await http.PostAsJsonAsync("/api/auth/resend-confirmation",new{email});
  async Task<HttpResponseMessage> Confirm(string token)=>await http.PostAsJsonAsync("/api/auth/confirm-email",new{token});
  string LinkToken(string marker)=>Uri.UnescapeDataString(mail.Body!.Split(marker)[1].Split('\n')[0]);
  async Task Switch(bool enabled){http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id="admin-fixture",Role=UserRole.SUPERADMIN}));var r=await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(enabled,"test@gmail.com","Academia","http://localhost:3017",null));if(!r.IsSuccessStatusCode)throw new Exception("Config switch failed");http.DefaultRequestHeaders.Authorization=null;}
  Check((await Register(input with{BirthDate=null})).StatusCode==HttpStatusCode.BadRequest,"Registro exige fecha de nacimiento");
  Check((await Register(input with{BirthDate=DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))})).StatusCode==HttpStatusCode.BadRequest,"Fecha de nacimiento futura rechazada");
  var signup=await Register(input);var signupBody=await signup.Content.ReadFromJsonAsync<JsonElement>();Check(signup.IsSuccessStatusCode&&signupBody.GetProperty("emailSent").GetBoolean(),"Registro guarda cuenta y envía confirmación por el servicio central");
  string userId;using(var scope=services.CreateScope()){var user=await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Users.SingleAsync(u=>u.Email==input.Email);userId=user.Id;Check(user.BirthDate==input.BirthDate&&user.EmailConfirmationRequired&&!user.EmailConfirmedAt.HasValue,"Nacimiento persistido y cuenta pendiente de confirmar");}
  var token=LinkToken("#confirm-email=");Check(mail.Body!.Contains("http://localhost:3017/#confirm-email=")&&!mail.Body.Contains("1996-02-29"),"Confirmación apunta al frontend sin incluir nacimiento en el mensaje");
  Check((await Login(input.Email)).StatusCode==HttpStatusCode.Forbidden,"Login bloqueado antes de confirmar el correo");
  var sent=mail.Count;var known=await Resend(input.Email);Check(known.IsSuccessStatusCode&&mail.Count==sent,"Reenvío inmediato no duplica correos: espera de un minuto");
  var unknown=await Resend("unknown@example.invalid");Check(unknown.IsSuccessStatusCode&&await known.Content.ReadAsStringAsync()==await unknown.Content.ReadAsStringAsync(),"Respuesta de reenvío no revela cuentas existentes");
  Check((await Confirm(token+"changed")).StatusCode==HttpStatusCode.BadRequest,"Enlace de confirmación manipulado rechazado");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var user=await db.Users.FindAsync(userId);var protector=scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>().CreateProtector("ForensicEdu.EmailConfirmation.v1").ToTimeLimitedDataProtector();var expired=protector.Protect(JsonSerializer.Serialize(new{UserId=user!.Id,Email=user.Email,Stamp=user.EmailConfirmationStamp}),TimeSpan.FromSeconds(-1));Check((await Confirm(expired)).StatusCode==HttpStatusCode.BadRequest,"Enlace de confirmación vencido rechazado");var reset=scope.ServiceProvider.GetRequiredService<IPasswordRecoveryTokens>().Create(user.Id,user.PasswordHash);Check((await Confirm(reset)).StatusCode==HttpStatusCode.BadRequest,"Token de recuperación no puede usarse para confirmar correo");}
  Check((await Confirm(token)).IsSuccessStatusCode,"Confirmación válida habilita la cuenta");
  Check((await Confirm(token)).StatusCode==HttpStatusCode.BadRequest,"Enlace de confirmación funciona una sola vez");
  Check((await Login(input.Email)).IsSuccessStatusCode,"Login habilitado tras confirmar");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.AuditLogs.AnyAsync(a=>a.UserId==userId&&a.Action=="EMAIL_CONFIRMED"),"Confirmación queda auditada sin tokens");}
  Check((await http.PostAsJsonAsync("/api/auth/forgot-password",new{email=input.Email})).IsSuccessStatusCode,"Recuperación sigue usando el mismo servicio de correo");
  var resetToken=LinkToken("#reset-password=");Check((await http.PostAsJsonAsync("/api/auth/reset-password",new{token=resetToken,password="ChangedPassword123!"})).IsSuccessStatusCode,"Formulario de recuperación cambia la contraseña");
  Check((await Login(input.Email)).StatusCode==HttpStatusCode.Unauthorized&&(await Login(input.Email,"ChangedPassword123!")).IsSuccessStatusCode,"Solo funciona la nueva contraseña recuperada");
  await Switch(false);sent=mail.Count;
  var disabled=input with{Email="disabled@example.invalid",Dni="DOC-REG-2"};var pending=await Register(disabled);var pendingBody=await pending.Content.ReadFromJsonAsync<JsonElement>();Check(pending.IsSuccessStatusCode&&!pendingBody.GetProperty("emailSent").GetBoolean()&&mail.Count==sent,"Con correos apagados se crea cuenta pendiente sin enviar mensajes");
  Check((await Login(disabled.Email)).StatusCode==HttpStatusCode.Forbidden,"Apagar correos no permite saltarse la confirmación");
  Check((await Resend(disabled.Email)).StatusCode==HttpStatusCode.ServiceUnavailable&&mail.Count==sent,"Interruptor desactivado bloquea reenvío de confirmación");
  Check((await http.PostAsJsonAsync("/api/auth/forgot-password",new{email=input.Email})).StatusCode==HttpStatusCode.ServiceUnavailable&&mail.Count==sent,"Mismo interruptor bloquea recuperación de contraseña");
  Check((await Login("student@example.invalid")).IsSuccessStatusCode,"Cuenta existente conserva acceso sin confirmación retroactiva");
  await Switch(true);Check((await Resend(disabled.Email)).IsSuccessStatusCode&&mail.Count==sent+1,"Al reactivar puede enviarse confirmación de la cuenta pendiente");
  var disabledToken=LinkToken("#confirm-email=");await Switch(false);Check((await Confirm(disabledToken)).IsSuccessStatusCode,"Desactivar envíos no invalida enlaces ya enviados");
  Check((await Login(disabled.Email)).IsSuccessStatusCode,"Cuenta confirmada puede entrar aunque se apaguen los envíos");
  await Switch(true);mail.Fail=true;
  var failure=await Register(input with{Email="delivery-failed@example.invalid",Dni="DOC-REG-3"});var failedBody=await failure.Content.ReadFromJsonAsync<JsonElement>();Check(failure.IsSuccessStatusCode&&!failedBody.GetProperty("emailSent").GetBoolean(),"Fallo SMTP conserva cuenta pendiente y permite recuperar el flujo");mail.Fail=false;
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var user=await db.Users.SingleAsync(u=>u.Email=="delivery-failed@example.invalid");Check(!user.EmailConfirmedAt.HasValue&&user.EmailConfirmationRequired,"Fallo de envío nunca confirma automáticamente al usuario");user.EmailConfirmationSentAt=DateTime.UtcNow.AddMinutes(-2);await db.SaveChangesAsync();}
  sent=mail.Count;Check((await Resend("delivery-failed@example.invalid")).IsSuccessStatusCode&&mail.Count==sent+1,"Puede reenviar tras corregir el problema SMTP y esperar");
  Console.WriteLine($"{count} comprobaciones de registro, confirmación y recuperación superadas sin correos reales.");
 }
}
