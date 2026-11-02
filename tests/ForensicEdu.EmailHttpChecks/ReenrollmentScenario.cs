using Microsoft.Extensions.Configuration;
using ForensicEdu.Infrastructure.Secrets;
using ForensicEdu.Application.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class ReenrollmentScenario
{
 public static async Task Seed(IServiceProvider services)
 {
  using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  if(!await db.Users.AnyAsync(u=>u.Id=="admin-fixture"))db.Users.Add(new User{Id="admin-fixture",Email="admin@example.invalid",FullName="Administrador de prueba",Role=UserRole.SUPERADMIN});
  var user=await db.Users.SingleAsync(u=>u.Id=="student");user.FullName="Alumno de prueba";
  db.Courses.Add(new Course{Id="reenroll-course",Title="Balística Forense",ShortCode="BAL",Price=100,IsActive=true,IsPublished=true,QuestionsJson="[{\"id\":\"q1\",\"question\":\"Pregunta de prueba\",\"options\":[\"Correcta\",\"Incorrecta\"],\"correctOptionIndex\":0}]"});
  db.CourseModules.Add(new CourseModule{Id="reenroll-module",CourseId="reenroll-course",Title="Fundamentos de balística",OrderIndex=1});
  AddFailedCycle(db,"student","old-cycle");
  db.SystemSocialSettings.Add(new SystemSocialSetting{Key="ExamRetakeWaitHours",Value="0"});
  await db.SaveChangesAsync();
 }
 private static void AddFailedCycle(ApplicationDbContext db,string userId,string id)
 {
  db.Orders.Add(new Order{Id=id+"-order",OrderCode=id+"-ORD",UserId=userId,ProductId="reenroll-course",ProductType=ProductType.CURSO,Amount=100,ProductTitle="Balística Forense",Status=OrderStatus.APROBADO});
  db.Enrollments.Add(new Enrollment{Id=id,UserId=userId,CourseId="reenroll-course",OrderId=id+"-order",EnrolledAt=DateTime.UtcNow.AddDays(-10),ProgressPercent=100,CompletedModulesJson="[\"reenroll-module\"]",AttemptsUsed=2,ExamStatus=ExamStatus.DESAPROBADO_DEFINITIVO,FirstAttemptScore=8,SecondAttemptScore=10,FinalGrade=10});
  for(var i=1;i<=2;i++)db.ExamAttempts.Add(new ExamAttempt{Id=id+"-attempt-"+i,EnrollmentId=id,UserId=userId,CourseId="reenroll-course",AttemptNumber=i,Score=i==1?8:10,Approved=false,CompletedAt=DateTime.UtcNow.AddDays(-3+i),Status=i==1?ExamStatus.DESAPROBADO_TEMPORAL:ExamStatus.DESAPROBADO_DEFINITIVO});
 }
 private static PaymentEvidence Pending(ApplicationDbContext db,string userId,string suffix)
 {
  var o=new Order{Id="pending-"+suffix,OrderCode="PENDING-"+suffix,UserId=userId,ProductId="reenroll-course",ProductTitle="Balística Forense",ProductType=ProductType.CURSO,Amount=100};db.Orders.Add(o);
  var p=new PaymentEvidence{Id="payment-"+suffix,OrderId=o.Id,Amount=100,BankName="BANK",OperationNumber=suffix,PaymentFingerprint=suffix,VoucherFileHash=suffix};db.PaymentEvidences.Add(p);return p;
 }
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens)
 {
  await Seed(services);var checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("OK: "+label);checks++;}
  void As(string id,int role)=>http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id=id,Role=(UserRole)role}));
  Task<HttpResponseMessage> Buy(string op)=>http.PostAsJsonAsync("/api/orders/checkout",new{userId="student",productId="reenroll-course",productType=1,amount=100,bankName="BCP",operationNumber=op,operationDate=DateTime.UtcNow.AddMinutes(-1),accountHolderName="Alumno de prueba"});
  As("student",6);var rows=await http.GetFromJsonAsync<JsonElement>("/api/student/enrollments");Check(rows[0].GetProperty("canReenroll").GetBoolean(),"Dos intentos terminados sin aprobar habilitan recompra");
  var bought=await Buy("REBUY-1");Check(bought.IsSuccessStatusCode,"Alumno puede registrar un nuevo pago después de agotar intentos");var result=await bought.Content.ReadFromJsonAsync<JsonElement>();var orderId=result.GetProperty("orderId").GetString()!;
  Check((await Buy("REBUY-1")).IsSuccessStatusCode,"Repetir la misma operación reutiliza la orden pendiente");
  Check((await Buy("REBUY-2")).StatusCode==HttpStatusCode.Conflict,"No registra otro pago diferente mientras se revisa el anterior");
  string paymentId;
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.Enrollments.CountAsync()==1,"Registrar pago no habilita aún una matrícula");var p=await db.PaymentEvidences.SingleAsync(p=>p.OrderId==orderId);paymentId=p.Id;p.VoucherFileHash="new-cycle-voucher";await db.SaveChangesAsync();}
  As("admin-fixture",1);Check((await http.PostAsync("/api/admin/payments/"+paymentId+"/approve",null)).IsSuccessStatusCode,"Aprobación crea un ciclo de matrícula nuevo");
  Check((await http.PostAsync("/api/admin/payments/"+paymentId+"/approve",null)).StatusCode==HttpStatusCode.BadRequest,"Repetir aprobación no entrega otro ciclo");
  string newId;
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var old=await db.Enrollments.FindAsync("old-cycle");var fresh=await db.Enrollments.SingleAsync(e=>e.OrderId==orderId);newId=fresh.Id;
   Check(old!.Status=="DESAPROBADA"&&old.FirstAttemptScore==8&&old.SecondAttemptScore==10&&old.ProgressPercent==100&&await db.ExamAttempts.CountAsync(a=>a.EnrollmentId==old.Id)==2,"Historial anterior conserva ambos intentos, notas y progreso");
   Check(fresh.Status=="ACTIVA"&&fresh.AttemptsUsed==0&&fresh.ProgressPercent==0&&fresh.CompletedModulesJson=="[]"&&fresh.FirstAttemptScore==null&&fresh.SecondAttemptScore==null&&fresh.FinalGrade==null&&fresh.SecondAttemptAvailableAt==null,"Matrícula nueva empieza limpia, sin heredar notas ni espera");
   Check(await db.Enrollments.CountAsync()==2&&await db.StudentNotifications.CountAsync(n=>n.Title=="Tu nueva matrícula está confirmada")==1,"Una nueva matrícula, aviso y auditoría por pago aprobado");
   Check(await db.AuditLogs.AnyAsync(a=>a.Action=="COURSE_REENROLLED"&&a.EntityId==newId),"Recompra queda auditada con el nuevo ID");
   Pending(db,"student","stale-before-feature");await db.SaveChangesAsync();
  }
  Check((await http.PostAsync("/api/admin/payments/payment-stale-before-feature/approve",null)).StatusCode==HttpStatusCode.Conflict,"Pago pendiente antiguo no crea otra matrícula sobre el ciclo activo");
  using(var scope=services.CreateScope()){Check((await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().PaymentEvidences.FindAsync("payment-stale-before-feature"))!.Status==PaymentStatus.PENDIENTE,"Aprobación rechazada no altera el comprobante");}
  As("student",6);Check((await Buy("REBUY-ACTIVE")).StatusCode==HttpStatusCode.Conflict,"No permite recomprar mientras haya intentos disponibles");
  Check((await http.GetAsync("/api/student/enrollments/old-cycle/classroom")).StatusCode==HttpStatusCode.Forbidden,"Ciclo anterior cerrado no modifica el aula actual");
  var history=await http.GetFromJsonAsync<JsonElement>("/api/student/enrollments/old-cycle/attempts");Check(history.GetArrayLength()==2,"Alumno consulta intentos anteriores por matrícula");
  Check((await http.GetAsync("/api/student/enrollments/"+newId+"/classroom")).IsSuccessStatusCode,"Aula nueva responde por su ID independiente");
  Check((await http.PostAsync("/api/exam-sessions/"+newId+"/start",null)).StatusCode==HttpStatusCode.BadRequest,"Nueva matrícula exige completar nuevamente los materiales");
  Check((await http.PostAsync("/api/exam-sessions/old-cycle/start",null)).StatusCode==HttpStatusCode.BadRequest,"No reabre una evaluación en la matrícula anterior");
  Check((await http.PutAsync("/api/student/enrollments/"+newId+"/modules/reenroll-module/complete",null)).IsSuccessStatusCode,"Progreso se registra en la nueva matrícula");
  var first=await http.PostAsync("/api/exam-sessions/"+newId+"/start",null);var session=await first.Content.ReadFromJsonAsync<JsonElement>();Check(first.IsSuccessStatusCode&&session.GetProperty("attemptNumber").GetInt32()==1,"Primera evaluación del nuevo ciclo vuelve a ser intento 1");
  Check((await http.PostAsJsonAsync("/api/exams/submit",new{enrollmentId=newId,userId="student",selectedAnswers=new Dictionary<string,int>{{"q1",1}}})).IsSuccessStatusCode,"Puede rendir primer intento del nuevo ciclo");
  var second=await http.PostAsync("/api/exam-sessions/"+newId+"/start",null);var secondSession=await second.Content.ReadFromJsonAsync<JsonElement>();Check(second.IsSuccessStatusCode&&secondSession.GetProperty("attemptNumber").GetInt32()==2,"Segundo intento nuevo no usa los intentos anteriores");
  Check((await http.PostAsJsonAsync("/api/exams/submit",new{enrollmentId=newId,userId="student",selectedAnswers=new Dictionary<string,int>{{"q1",0}}})).IsSuccessStatusCode,"Nueva matrícula permite aprobar y obtener certificado");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.DigitalCertificates.AnyAsync(c=>c.EnrollmentId==newId)&&!await db.DigitalCertificates.AnyAsync(c=>c.EnrollmentId=="old-cycle")&&await db.ExamAttempts.CountAsync(a=>a.UserId=="student")==4,"Certificado pertenece al ciclo aprobado y se conservan cuatro intentos históricos");}
  Check((await Buy("REBUY-PASSED")).StatusCode==HttpStatusCode.Conflict,"Curso aprobado no habilita otra recompra por su historia anterior");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Users.Add(new User{Id="racing-student",Email="race@example.invalid"});AddFailedCycle(db,"racing-student","racing-old");Pending(db,"racing-student","race-1");Pending(db,"racing-student","race-2");await db.SaveChangesAsync();}
  As("admin-fixture",1);var concurrent=await Task.WhenAll(http.PostAsync("/api/admin/payments/payment-race-1/approve",null),http.PostAsync("/api/admin/payments/payment-race-2/approve",null));
  Check(concurrent.Count(r=>r.IsSuccessStatusCode)==1&&concurrent.Count(r=>r.StatusCode==HttpStatusCode.Conflict)==1,"Aprobaciones simultáneas de dos pagos entregan un solo ciclo nuevo");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.Enrollments.CountAsync(e=>e.UserId=="racing-student"&&e.Status=="ACTIVA")==1,"Solo queda una matrícula activa para alumno y curso");}
  As("racing-student",6);Check((await http.GetAsync("/api/student/enrollments/old-cycle/attempts")).StatusCode==HttpStatusCode.NotFound,"No permite consultar historial de otro alumno");
  using(var scope=services.CreateScope())
  {
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   db.Users.Add(new User{Id="counter-only",Email="counter@example.invalid"});AddFailedCycle(db,"counter-only","counter-cycle");await db.SaveChangesAsync();
   var unfinished=await db.ExamAttempts.FindAsync("counter-cycle-attempt-2");unfinished!.CompletedAt=null;await db.SaveChangesAsync();
   Check(!(await CourseEnrollmentRules.CheckAsync(db,"counter-only","reenroll-course",default)).Allowed,"Contador de dos intentos no habilita recompra con examen aún abierto");
   unfinished.CompletedAt=DateTime.UtcNow;var blockedCycle=await db.Enrollments.FindAsync("counter-cycle");blockedCycle!.Status="BLOQUEADA";await db.SaveChangesAsync();
   Check(!(await CourseEnrollmentRules.CheckAsync(db,"counter-only","reenroll-course",default)).Allowed,"Recompra no evita un bloqueo administrativo");
  }
  Console.WriteLine($"{checks} comprobaciones de nueva matrícula superadas sin cobros ni correos reales.");
 }
 public static async Task CheckPostgresLock()
 {
  // Only transaction advisory locks: no tables or user data are created or modified.
  var file=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Microsoft","UserSecrets","ForensicEdu-Local-Development","secrets.json");
  var config=new ConfigurationManager();config.SetBasePath(Path.GetFullPath("src/ForensicEdu.WebApi"));
  config.AddJsonFile("appsettings.json").AddUserSecrets("ForensicEdu-Local-Development").AddJsonFile("appsettings.Local.json",optional:true).AddEnvironmentVariables();
  using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(30));
  await config.AddOpenBaoSecretsAsync(timeout.Token);config.AddEnvironmentVariables();var connection=config.GetConnectionString("DefaultConnection");
  await using var a=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
  await using var b=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
  var firstEntered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var secondEntered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var key=Guid.NewGuid().ToString();
  var one=new CoursePurchaseGate(a).ExecuteAsync(key,"probe",async()=>{firstEntered.SetResult();await release.Task;return true;},timeout.Token);
  await Task.WhenAny(firstEntered.Task,one);if(one.IsCompleted)await one;await firstEntered.Task;var two=new CoursePurchaseGate(b).ExecuteAsync(key,"probe",()=>{secondEntered.SetResult();return Task.FromResult(true);},timeout.Token);
  await Task.Delay(300,timeout.Token);var blocked=!secondEntered.Task.IsCompleted;release.SetResult();await Task.WhenAll(one,two);
  if(!blocked||!secondEntered.Task.IsCompleted)throw new Exception("El bloqueo PostgreSQL no serializó las operaciones.");
  Console.WriteLine("OK: PostgreSQL serializa dos conexiones y libera el bloqueo al confirmar. Sin escrituras en tablas.");
 }
}
