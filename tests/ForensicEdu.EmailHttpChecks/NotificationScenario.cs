using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Services;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class NotificationScenario
{
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens)
 {
  var count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  void As(string id,int role)=>http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id=id,Role=(UserRole)role}));
  async Task Dispatch(){using var scope=services.CreateScope();await scope.ServiceProvider.GetRequiredService<NotificationEmailDispatcher>().DispatchAsync();}
  async Task<StudentNoticePage> List()=>await http.GetFromJsonAsync<StudentNoticePage>("/api/student/notifications")??throw new Exception("Missing inbox");
  Check((await http.GetAsync("/api/student/notifications")).StatusCode==HttpStatusCode.Unauthorized,"Bandeja exige autenticación");
  await AdvisoryCareScenario.Seed(services);
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   db.Courses.Add(new Course{Id="notice-course",Title="Balística Forense",Price=250});
   var o=await db.Orders.FindAsync("care-order-1");o!.ProductType=ProductType.CURSO;o.ProductId="notice-course";o.ProductTitle="Balística Forense";
   db.SupportTickets.Add(new SupportTicket{Id="notice-ticket",TicketCode="TKT-TEST",UserId="student",UserEmail="student@example.invalid",Subject="Acceso al aula"});
   await db.SaveChangesAsync();
  }
  As("admin-fixture",1);
  Check((await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(false,"test@gmail.com","Academia","http://localhost:3017",null))).IsSuccessStatusCode,"Administrador desactiva correo central");
  Check((await http.PostAsync("/api/admin/payments/care-payment-1/approve",null)).IsSuccessStatusCode,"Aprobación confirma matrícula con correo apagado");
  await http.PostAsync("/api/admin/payments/care-payment-1/approve",null);
  Check((await http.PostAsJsonAsync("/api/student/tickets/notice-ticket/reply",new{message="Tu acceso fue habilitado. <script>no ejecutar</script>"})).IsSuccessStatusCode,"Respuesta de soporte crea aviso");
  As("student",6);var list=await List();
  Check(list.Total==2&&list.UnreadCount==2&&list.Items.Any(n=>n.Kind=="MATRICULA")&&list.Items.Any(n=>n.Kind=="SOPORTE"),"Alumno recibe dos avisos persistentes, sin duplicar aprobación");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.Enrollments.CountAsync(e=>e.UserId=="student")==1,"Matrícula y notificación persisten juntas");}
  var own=list.Items[0].Id;
  As("other-student",6);Check((await List()).Total==0,"Bandeja no expone avisos de otro alumno");
  Check((await http.PutAsync("/api/student/notifications/"+own+"/read",null)).StatusCode==HttpStatusCode.NotFound,"ID ajeno no permite marcar ni revelar aviso");
  As("student",6);Check((await http.PutAsync("/api/student/notifications/"+own+"/read",null)).IsSuccessStatusCode,"Alumno puede marcar un aviso propio");
  Check((await List()).UnreadCount==1,"Contador y estado leído se guardan en API");
  await http.PutAsync("/api/student/notifications/read-all",null);Check((await List()).UnreadCount==0,"Marcar todos respeta la cuenta actual");
  var capture=services.GetRequiredService<MailCapture>();var before=capture.Count;
  await Dispatch();Check(capture.Count==before,"No sale correo automático con envío desactivado");
  As("admin-fixture",1);await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(true,"test@gmail.com","Academia","http://localhost:3017",null));
  await Dispatch();Check(capture.Count==before+2,"Al activar se procesan avisos pendientes");
  await Dispatch();Check(capture.Count==before+2,"Correos enviados no se repiten al ejecutar el trabajo");
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   for(var i=0;i<3;i++){var left=new[]{6,2,0}[i];db.Users.Add(new User{Id="reminder-"+i,Email="reminder"+i+"@example.invalid",RegisteredAt=DateTime.UtcNow.AddDays(-30+left).AddHours(12)});}
   db.Users.Add(new User{Id="expired-paid",Email="expired@example.invalid",HasApprovedPurchase=true,ValidUntil=DateTime.UtcNow.AddDays(-1)});
   db.Users.Add(new User{Id="staff-expired",Role=UserRole.SUPERADMIN,Email="staff@example.invalid",ValidUntil=DateTime.UtcNow.AddYears(-1)});
   await db.SaveChangesAsync();await new AccountLifecycleService(db).ProcessDailyAccountExpirationsAsync();await new AccountLifecycleService(db).ProcessDailyAccountExpirationsAsync();
   Check(await db.StudentNotifications.CountAsync(n=>n.Kind=="VIGENCIA")==3,"Avisos de ventanas 7/3/1 días no se duplican en auditorías repetidas");
   Check((await db.Users.FindAsync("expired-paid"))!.Status==AccountStatus.SUSPENDIDO&&await db.StudentNotifications.CountAsync(n=>n.UserId=="expired-paid")==1,"Vencimiento comprado cambia estado y genera un aviso");
   Check((await db.Users.FindAsync("staff-expired"))!.Status==AccountStatus.ACTIVO,"Reglas de alumnos no suspenden al administrador");
   await new AccountLifecycleService(db).ExtendValidityOnApprovedPaymentAsync("reminder-0",DateTime.UtcNow);
   Check(await db.StudentNotifications.AnyAsync(n=>n.UserId=="reminder-0"&&n.Kind=="VIGENCIA"&&n.EmailStatus=="CANCELLED"),"Renovar cancela el correo de vencimiento anterior");
   StudentNotifications.Add(db,"student","expired-mail","VIGENCIA","Aviso antiguo","No reenviar","perfil",DateTime.UtcNow.AddMinutes(-1));
   await db.SaveChangesAsync();
  }
  capture.Fail=true;await Dispatch();
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   Check(await db.StudentNotifications.AnyAsync(n=>n.EventKey=="expired-mail"&&n.EmailStatus=="CANCELLED"),"Avisos fuera de vigencia no se reenvían");
   var failed=await db.StudentNotifications.Where(n=>n.EmailStatus=="PENDING"&&n.EmailAttempts>0).ToListAsync();
   Check(failed.Count>0&&failed.All(n=>n.NextEmailAttemptAt>DateTime.UtcNow),"Fallo SMTP conserva avisos y programa reintento sin revertir datos");
   foreach(var n in failed)n.NextEmailAttemptAt=DateTime.UtcNow.AddMinutes(-1);await db.SaveChangesAsync();
  }
  capture.Fail=false;await Dispatch();
  using(var scope=services.CreateScope()){Check(!await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().StudentNotifications.AnyAsync(n=>n.EmailStatus=="PENDING"&&n.EmailAttempts>0),"Reintento posterior completa los envíos");}
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   db.Users.Add(new User{Id="unverified-notice",Email="verify@example.invalid",EmailConfirmationRequired=true});
   StudentNotifications.Add(db,"unverified-notice","verify-notice","MATRICULA","Acceso","Tu compra está lista.","mis-cursos");
   db.StudentNotifications.Add(new StudentNotification{UserId="student",EventKey="retry-exhausted",Title="Prueba de reintento",Body="Aviso de prueba",EmailStatus="FAILED",EmailAttempts=5});
   for(var i=0;i<21;i++)db.StudentNotifications.Add(new StudentNotification{UserId="other-student",EventKey="page-"+i,Title="Aviso "+i,Body="Prueba de página",EmailStatus="CANCELLED"});
   await db.SaveChangesAsync();
  }
  before=capture.Count;await Dispatch();Check(capture.Count==before,"Direcciones pendientes de confirmar no reciben avisos educativos");
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var u=await db.Users.FindAsync("unverified-notice");u!.EmailConfirmedAt=DateTime.UtcNow;
   var n=await db.StudentNotifications.SingleAsync(n=>n.EventKey=="verify-notice");n.NextEmailAttemptAt=null;await db.SaveChangesAsync();
  }
  await Dispatch();Check(capture.Count==before+1,"Confirmar la dirección permite procesar el aviso pendiente");
  As("admin-fixture",1);await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(true,"test@gmail.com","Academia","http://localhost:3017",null));
  await Dispatch();Check(capture.Count==before+2,"Guardar configuración habilitada libera los correos con reintentos agotados");
  As("other-student",6);var pages=await List();var secondPage=await http.GetFromJsonAsync<StudentNoticePage>("/api/student/notifications?page=2");
  Check(pages.Items.Count==20&&pages.Total==21&&secondPage!.Items.Count==1,"La bandeja pagina avisos propios en grupos de 20");
  await http.PutAsync("/api/student/notifications/read-all",null);
  var emptyUnread=await http.GetFromJsonAsync<StudentNoticePage>("/api/student/notifications?unreadOnly=true");
  Check(emptyUnread!.Total==0&&emptyUnread.UnreadCount==0,"Filtro sin leer refleja lectura persistente en todas las páginas");
  var html=PlatformEmailTemplate.Render(new("<script>alert(1)</script>","Texto & <img src=x>","Recuperar","https://example.invalid/#reset-password=fake","Válido durante 30 minutos","Si no solicitaste este cambio, ignora el correo."));
  Check(!html.Contains("<script>")&&!html.Contains("<img src=x>")&&html.Contains("&lt;script&gt;"),"Plantilla codifica títulos y mensajes para impedir HTML inyectado");
  Check(!html.Contains("¿El botón")&&!html.Contains("Tu cuenta, tu progreso")&&html.Contains("#reset-password=fake"),"Diseño aprobado conserva acción y omite el pie descartado");
  Check(!PlatformEmailTemplate.Render(new("X","Y","Abrir","javascript:alert(1)")).Contains("href="),"Plantilla rechaza enlaces no HTTP/HTTPS");
  Directory.CreateDirectory(".artifacts/notification-preview");await File.WriteAllTextAsync(".artifacts/notification-preview/recuperacion.html",PlatformEmailTemplate.Render(new("Vuelve a tu formación.","Recibimos una solicitud para restablecer la contraseña de tu cuenta. Usa el siguiente botón para crear una nueva y continuar aprendiendo.","Restablecer mi contraseña","https://example.invalid/#reset-password=ejemplo","Válido durante 30 minutos\nEste enlace solo puede utilizarse una vez.","Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña actual seguirá siendo la misma.","RECUPERACIÓN DE ACCESO")));
  Console.WriteLine($"{count} comprobaciones de notificaciones superadas. Solo memoria y correo simulado.");
 }
}
