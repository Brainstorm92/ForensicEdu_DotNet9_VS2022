using ForensicEdu.Infrastructure.Resilience;
using ForensicEdu.Application;
using ForensicEdu.WebApi.Services;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Services;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using ForensicEdu.WebApi.Controllers;
using ForensicEdu.WebApi.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

if(args.Contains("--video-storage-check")||args.Contains("--video-storage-cors")||args.Contains("--video-storage-live")){await VideoStorageScenario.Run(args);return;}
if(args.Contains("--reenrollment-lock")){await ReenrollmentScenario.CheckPostgresLock();return;}
var builder=WebApplication.CreateBuilder(new WebApplicationOptions {EnvironmentName="Testing"});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls(args.Contains("--serve")?"http://127.0.0.1:5097":"http://127.0.0.1:0");
builder.Configuration.AddInMemoryCollection(new Dictionary<string,string?> { ["Email:Enabled"]="true",["Email:Address"]="test@gmail.com",["Email:Password"]="fixture-only-password",["Email:FrontendUrl"]="http://localhost:3017",["JwtSettings:Issuer"]="EmailChecks",["JwtSettings:Audience"]="EmailChecks",["JwtSettings:Secret"]="Only-a-local-fixture-key-not-a-production-credential-2026" });
builder.Services.AddControllers().AddApplicationPart(typeof(EmailSettingsController).Assembly).ConfigureApplicationPartManager(p=>p.FeatureProviders.Add(new OnlyEmailControllers()));
builder.Services.AddDbContext<ApplicationDbContext>(o=>o.UseInMemoryDatabase("email-http-"+Environment.ProcessId));
builder.Services.AddScoped<ICoursePurchaseGate,CoursePurchaseGate>();
builder.Services.AddScoped<IApplicationDbContext>(s=>s.GetRequiredService<ApplicationDbContext>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser,CurrentUser>();
builder.Services.AddApplicationServices();
builder.Services.AddDatabaseResilience(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
builder.Services.AddScoped<IPasswordService,PasswordService>();
builder.Services.AddScoped<IPersistenceErrors,PersistenceErrors>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IEmailConfirmationTokens,EmailConfirmationTokens>();
builder.Services.AddScoped<IEmailConfirmation,EmailConfirmationService>();
builder.Services.AddScoped<IPasswordRecoveryTokens,PasswordRecoveryTokens>();
builder.Services.AddScoped<IPasswordRecovery,PasswordRecoveryService>();
builder.Services.AddScoped<GmailPlatformEmail>();
 builder.Services.AddScoped<NotificationEmailDispatcher>();
builder.Services.AddScoped<IVideoUploadTokens,VideoUploadTokens>();
builder.Services.AddSingleton<FakeVideoStorage>();builder.Services.AddSingleton<IPrivateVideoStorage>(s=>s.GetRequiredService<FakeVideoStorage>());
builder.Services.AddCors(o=>o.AddPolicy("video-fixture",p=>p.WithOrigins("http://127.0.0.1:3017").WithMethods("GET","PUT","HEAD").AllowAnyHeader().WithExposedHeaders("Content-Range","Accept-Ranges","Content-Length")));
builder.Services.AddSingleton<MailCapture>();
builder.Services.AddScoped<IPlatformEmail,RecordingMail>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new TokenValidationParameters {ValidateIssuer=true,ValidateAudience=true,ValidateLifetime=true,ValidateIssuerSigningKey=true,ValidIssuer="EmailChecks",ValidAudience="EmailChecks",IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))});
builder.Services.AddAuthorization();builder.Services.AddEmailRateLimits();
if(args.Contains("--registration-ui")){builder.Configuration["Email:Enabled"]="false";builder.Configuration["Email:FrontendUrl"]="http://127.0.0.1:3017/.artifacts/access-test.html";}
await using var app=builder.Build();app.UseExceptionHandler();app.UseCors("video-fixture");app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();app.MapControllers();
using(var scope=app.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Users.Add(new User {Id="student",Email="student@example.invalid",PasswordHash=new PasswordService().Hash("OriginalPassword123!"),Status=AccountStatus.ACTIVO});await db.SaveChangesAsync();}
if(args.Contains("--care")){
 await AdvisoryCareScenario.Seed(app.Services);
 using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var order=await db.Orders.FindAsync("care-order-1");order!.Status=OrderStatus.APROBADO;
 db.AdvisoryCases.Add(new AdvisoryCase{Id="fixture-care",OrderId=order.Id,AdvisoryServiceId=order.ProductId,StudentId="student"});db.AdvisoryCareHistory.Add(new(){AdvisoryCaseId="fixture-care",ActorId="admin-fixture",Action="ADVISORY_CARE_CREATED",Details="Compra aprobada. Pendiente de asignación de consultor."});await db.SaveChangesAsync();
 app.MapGet("/api/fixture/tokens",()=>{var tokens=new TokenService(builder.Configuration);return new{admin=tokens.Create(new User{Id="admin-fixture",Role=UserRole.SUPERADMIN}),consultant=tokens.Create(new User{Id="consultant-1",Role=UserRole.CONSULTOR}),student=tokens.Create(new User{Id="student",Role=UserRole.ESTUDIANTE})};});
}
if(args.Contains("--serve")){using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.LiveEvents.Add(new LiveEvent{Id="fixture-live",Title="Seminario en vivo",EventDate=DateTime.UtcNow.AddMinutes(-2),AccessStatus="EN_VIVO",RoomUrl="https://example.com/?event-access-test=1"});db.Orders.Add(new Order{Id="fixture-order",UserId="admin-fixture",ProductId="fixture-live",ProductType=ProductType.EVENTO,Status=OrderStatus.APROBADO});await db.SaveChangesAsync();}
if(args.Contains("--notifications-ui")){
 using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
 var user=await db.Users.FindAsync("student");user!.RegisteredAt=DateTime.UtcNow.AddDays(-24);user.ValidUntil=DateTime.UtcNow.AddDays(6);
 ForensicEdu.Application.Services.StudentNotifications.Add(db,"student","preview-enrollment","MATRICULA","Tu matrícula está confirmada","Aprobamos tu pago del curso de Balística Forense. Ya puedes consultar tu aula y continuar con tu formación.","mis-cursos");
 ForensicEdu.Application.Services.StudentNotifications.Add(db,"student","preview-support","SOPORTE","Tienes una respuesta de soporte","Respondimos a tu consulta TKT-2026-001: acceso al aula.\n\nHola, tu acceso está habilitado. Ingresa a Mis Cursos y Evaluaciones para continuar.","mis-consultas");
 await db.SaveChangesAsync();await new AccountLifecycleService(db).ProcessDailyAccountExpirationsAsync();
 app.MapGet("/api/fixture/student-token",()=>new{token=new TokenService(builder.Configuration).Create(new User{Id="student",Role=UserRole.ESTUDIANTE})});
}
if(args.Contains("--serve"))app.MapPut("/fixture/room",async (RoomFixture request,ApplicationDbContext db)=>{var room=await db.LiveEvents.FindAsync("fixture-live");room!.RoomUrl=request.Url;await db.SaveChangesAsync();return Microsoft.AspNetCore.Http.Results.NoContent();}).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute{Roles="1"});
if(args.Contains("--registration-ui")){
 using(var scope=app.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Users.Add(new User{Id="pending-ui",FullName="Alumna de prueba",Nombres="Alumna",Apellidos="Prueba",Email="pending-ui@example.invalid",BirthDate=new DateOnly(1996,2,29),EmailConfirmationRequired=true,PasswordHash=new PasswordService().Hash("OriginalPassword123!"),Role=UserRole.ESTUDIANTE});await db.SaveChangesAsync();}
 app.MapGet("/api/auth/google/config",()=>new{enabled=false,clientId="",nonce=""});
 app.MapGet("/api/fixture/admin-token",()=>new{token=new TokenService(builder.Configuration).Create(new User{Id="admin-fixture",Role=UserRole.SUPERADMIN})});
 app.MapGet("/api/fixture/mail",()=>{var capture=app.Services.GetRequiredService<MailCapture>();var link=capture.Body?.Split('\n').FirstOrDefault(x=>x.StartsWith("http"));return new{link,count=capture.Count};});
}
VideoScenario.MapStorage(app);
if(args.Contains("--video-ui")){
 await VideoScenario.Seed(app.Services);
 if(args.Contains("--classroom-ui")){
  using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  var course=(await db.Courses.FindAsync("video-course"))!;course.Title="Balística Forense";course.ShortCode="CF-BAL-2026";course.AcademicHours=60;course.DurationHours=60;course.Instructor="Equipo académico";course.ImageUrl="/assets/course-placeholder.svg";
  var first=(await db.CourseModules.FindAsync("video-module"))!;first.Title="Fundamentos de Balística Interior y Armamento";first.Duration="1h 45m";first.StreamingId="videos/11111111111111111111111111111111.mp4";
  (await db.CourseModules.FindAsync("video-module-2"))!.Title="Balística Exterior y Reconstrucción de Trayectorias";
  db.CourseModules.AddRange(new CourseModule{Id="video-module-3",CourseId=course.Id,Title="Balística Terminal y Heridas por Arma de Fuego",OrderIndex=2},new CourseModule{Id="video-module-4",CourseId=course.Id,Title="Cotejo Microscópico y Emisión del Informe",OrderIndex=3});
  db.CourseDocuments.AddRange(new CourseDocument{Id="fixture-document-1",ModuleId=first.Id,Title="Fundamentos de Balística Interior.pdf",FileType="PDF",FileSize="4.8 MB"},new CourseDocument{Id="fixture-document-2",ModuleId=first.Id,Title="Glosario de Municiones y Cartuchería.docx",FileType="DOCX",FileSize="1.2 MB"});
  var enrollment=(await db.Enrollments.FindAsync("video-enrollment"))!;enrollment.ProgressPercent=25;enrollment.CompletedModulesJson="[\"video-module\"]";
  var student=(await db.Users.FindAsync("student"))!;student.FullName="Alumno de prueba";student.Nombres="Alumno";student.Apellidos="de prueba";
  await db.SaveChangesAsync();
 }

 app.MapGet("/api/fixture/student-token",()=>new{token=new TokenService(builder.Configuration).Create(new User{Id="student",Role=UserRole.ESTUDIANTE})});
}
if(args.Contains("--reenrollment-ui")){
 await ReenrollmentScenario.Seed(app.Services);
 app.MapGet("/api/fixture/student-token",()=>new{token=new TokenService(builder.Configuration).Create(new User{Id="student",Role=UserRole.ESTUDIANTE})});
 app.MapPut("/api/fixture/reenrollment-voucher/{id}",async(string id,ApplicationDbContext db)=>{var p=await db.PaymentEvidences.SingleAsync(p=>p.OrderId==id);p.VoucherFileHash="fixture-voucher-"+id;await db.SaveChangesAsync();return Microsoft.AspNetCore.Http.Results.NoContent();}).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute{Roles="1"});
}
await app.StartAsync();
var tokenService=new TokenService(builder.Configuration);
var admin=tokenService.Create(new User {Id="admin-fixture",Role=(UserRole)1});
if(args.Contains("--serve"))
{
    Directory.CreateDirectory(".artifacts");await File.WriteAllTextAsync(".artifacts/email-http-token.txt",admin);
    Console.WriteLine("Servidor de prueba aislado en 127.0.0.1:5097. Datos en memoria; no conecta a PostgreSQL ni a Gmail.");
    await app.WaitForShutdownAsync();return;
}
var url=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var http=new HttpClient {BaseAddress=new Uri(url)};
if(args.Contains("--videos")){await VideoScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--reenrollment")){await ReenrollmentScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--notifications")){await NotificationScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--registration-email")){await RegistrationEmailScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--advisory-care")){await AdvisoryCareScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--event-access")){await EventAccessScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
if(args.Contains("--catalog")){await CatalogScenario.Run(http,app.Services,tokenService);await app.StopAsync();return;}
var count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
Check((await http.GetAsync("/api/admin/email")).StatusCode==HttpStatusCode.Unauthorized,"Configuración bloqueada sin sesión");
http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokenService.Create(new User{Id="academic",Role=(UserRole)2}));
Check((await http.GetAsync("/api/admin/email")).StatusCode==HttpStatusCode.Forbidden,"Administrador académico no puede leer credenciales");
Check((await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(false,"test@gmail.com","Academia","http://localhost:3017",null))).StatusCode==HttpStatusCode.Forbidden,"Otro administrador no puede modificar correo");
Check((await http.PostAsync("/api/admin/email/test",null)).StatusCode==HttpStatusCode.Forbidden,"Otro administrador no puede enviar pruebas");
http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",admin);
var response=await http.GetAsync("/api/admin/email");var body=await response.Content.ReadAsStringAsync();
Check(response.IsSuccessStatusCode&&!body.Contains("fixture-only-password")&&response.Headers.CacheControl?.NoStore==true,"Superadministrador recibe estado sin clave y sin caché");
Check((await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(false,"test@gmail.com","Academia","http://localhost:3017",null))).StatusCode==HttpStatusCode.NoContent,"Panel puede apagar envío");
Check((await http.PostAsJsonAsync("/api/auth/forgot-password",new{email="student@example.invalid"})).StatusCode==HttpStatusCode.ServiceUnavailable,"API pública respeta envío desactivado");
Check((await http.PostAsync("/api/admin/email/test",null)).IsSuccessStatusCode,"Prueba manual está disponible con envío automático apagado");
await http.PutAsJsonAsync("/api/admin/email",new UpdateEmailConfiguration(true,"test@gmail.com","Academia","http://localhost:3017",null));
var unknown=await http.PostAsJsonAsync("/api/auth/forgot-password",new{email="unknown@example.invalid"});
var known=await http.PostAsJsonAsync("/api/auth/forgot-password",new{email="student@example.invalid"});
Check(unknown.IsSuccessStatusCode&&known.IsSuccessStatusCode&&await unknown.Content.ReadAsStringAsync()==await known.Content.ReadAsStringAsync(),"Respuestas HTTP no distinguen correos existentes");
var captured=app.Services.GetRequiredService<MailCapture>().Body!;
var token=Uri.UnescapeDataString(captured.Split("#reset-password=")[1].Split('\n')[0]);
Check((await http.PostAsJsonAsync("/api/auth/reset-password",new{token="invalid",password="NewPassword123!"})).StatusCode==HttpStatusCode.BadRequest,"Endpoint rechaza enlace inválido");
Check((await http.PostAsJsonAsync("/api/auth/reset-password",new{token,password="NewPassword123!"})).IsSuccessStatusCode,"Restablecimiento completo mediante HTTP");
Check((await http.PostAsJsonAsync("/api/auth/reset-password",new{token,password="AnotherPassword123!"})).StatusCode==HttpStatusCode.BadRequest,"HTTP impide reutilización del enlace");
var limited=false;for(var i=0;i<10;i++){var r=await http.PostAsJsonAsync("/api/auth/forgot-password",new{email="unknown@example.invalid"});if((int)r.StatusCode==429){limited=r.Headers.RetryAfter!=null;break;}}
Check(limited,"Solicitudes repetidas limitadas con 429 y Retry-After");
await app.StopAsync();Console.WriteLine($"{count} comprobaciones HTTP superadas sin base remota ni correos reales.");

sealed class OnlyEmailControllers:IApplicationFeatureProvider<ControllerFeature>{public void PopulateFeature(IEnumerable<ApplicationPart> parts,ControllerFeature feature){foreach(var t in feature.Controllers.Where(t=>t.AsType()!=typeof(CourseVideosController)&&t.AsType()!=typeof(ExamSessionsController)&&t.AsType()!=typeof(ExamsController)&&t.AsType()!=typeof(StudentWorkspaceController)&&t.AsType()!=typeof(StudentNotificationsController)&&t.AsType()!=typeof(EmailSettingsController)&&t.AsType()!=typeof(PasswordRecoveryController)&&t.AsType()!=typeof(CatalogAdminController)&&t.AsType()!=typeof(StudentServicesController)&&t.AsType()!=typeof(AdvisoryCareController)&&t.AsType()!=typeof(AdminController)&&t.AsType()!=typeof(AuthController)&&t.AsType()!=typeof(EmailConfirmationController)&&t.AsType()!=typeof(OperationsAdminController)&&t.AsType()!=typeof(PublicCatalogController)&&t.AsType()!=typeof(CoursesController)&&t.AsType()!=typeof(SettingsController)&&t.AsType()!=typeof(OrdersController)).ToArray())feature.Controllers.Remove(t);}}
sealed class Actor:ICurrentUser{public string UserId=>"admin-fixture";public bool IsInRole(string r)=>r=="1";}
sealed class MailCapture{public string? Body;public int Count;public bool Fail;}
sealed class RecordingMail(GmailPlatformEmail config,MailCapture capture):IPlatformEmail
{
    public Task<EmailConfiguration> GetConfigurationAsync(CancellationToken ct)=>config.GetConfigurationAsync(ct);
    public Task SaveConfigurationAsync(UpdateEmailConfiguration r,CancellationToken ct)=>config.SaveConfigurationAsync(r,ct);
    public Task TestAsync(CancellationToken ct)=>Task.CompletedTask;
    public async Task SendAsync(string recipient,string subject,string body,CancellationToken ct){if(!(await config.GetConfigurationAsync(ct)).Enabled)throw new ForensicEdu.Application.Common.Exceptions.EmailDeliveryUnavailableException("EMAIL_DISABLED");if(capture.Fail)throw new System.Net.Mail.SmtpException("Synthetic mail failure");capture.Body=body;capture.Count++;}
}

record RoomFixture(string Url);
