using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using ForensicEdu.Infrastructure.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

static class PaymentAntifraudScenario
{
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens)
 {
  int checks=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("OK: "+label);}
  string Token(string user,UserRole role=UserRole.ESTUDIANTE)=>tokens.Create(new User{Id=user,Role=role});var admin=Token("fraud-admin",UserRole.SUPERADMIN);
  async Task<HttpResponseMessage> Send(HttpMethod method,string path,string token,object? body=null){using var req=new HttpRequestMessage(method,path);req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);if(body!=null)req.Content=JsonContent.Create(body);return await http.SendAsync(req);}
  async Task<JsonElement> Assess(string id){var r=await Send(HttpMethod.Get,"/api/admin/payments/"+id,admin);r.EnsureSuccessStatusCode();return (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("antifraud");}
  Task<HttpResponseMessage> Approve(string id,string? token=null,string? reason=null)=>Send(HttpMethod.Post,"/api/admin/payments/"+id+"/approve",admin,token==null?null:new{reviewToken=token,reviewReason=reason});
  async Task Add(string id,string owner,string product,ProductType type,string operation,string hash,string holder="",string bank="BCP",PaymentStatus status=PaymentStatus.PENDIENTE){using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var o=new Order{Id="o-"+id,OrderCode="ORD-"+id,UserId=owner,ProductId=product,ProductType=type,Amount=100,Price=100,Status=OrderStatus.PENDIENTE_VALIDACION};db.Orders.Add(o);db.PaymentEvidences.Add(new PaymentEvidence{Id=id,OrderId=o.Id,BankName=bank,OperationNumber=operation,Amount=100,OperationDate=DateTime.UtcNow.Date,AccountHolderName=holder==""?id:holder,VoucherFileHash=hash,VoucherFileUrl="fixture-"+id,Status=status});await db.SaveChangesAsync();}
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Users.AddRange(new User{Id="fraud-admin",Email="fraud-admin@example.invalid",Role=UserRole.SUPERADMIN},new User{Id="fraud-a",Email="fraud-a@example.invalid",Status=AccountStatus.ACTIVO},new User{Id="fraud-b",Email="fraud-b@example.invalid",Status=AccountStatus.ACTIVO});for(int i=1;i<=10;i++)db.Courses.Add(new Course{Id="fraud-course-"+i,Title="Curso antifraude "+i,Price=100});db.LiveEvents.Add(new LiveEvent{Id="fraud-event",Title="Evento antifraude",Price=100});db.AdvisoryServices.Add(new AdvisoryService{Id="fraud-advisory",Title="Asesoría antifraude",Price=100});await db.SaveChangesAsync();}
  var a=Token("fraud-a");var b=Token("fraud-b");
  object Checkout(string owner,string product,int type,string op)=>new{userId=owner,productId=product,productType=type,amount=100,bankName=" bcp ",operationNumber=op,operationDate=DateTime.UtcNow.AddMinutes(-1),accountHolderName=owner};
  var race=await Task.WhenAll(Send(HttpMethod.Post,"/api/orders/checkout",a,Checkout("fraud-a","fraud-course-1",1,"001-234")),Send(HttpMethod.Post,"/api/orders/checkout",b,Checkout("fraud-b","fraud-event",2,"001 234")));
  Check(race.Count(r=>r.IsSuccessStatusCode)==1&&race.Count(r=>r.StatusCode==HttpStatusCode.Conflict)==1,"Checkout simultáneo entre usuarios y productos reserva una sola operación normalizada");
  await Add("operation-a","fraud-a","fraud-course-2",ProductType.CURSO,"OP-991","hash-a");await Add("operation-b","fraud-b","fraud-event",ProductType.EVENTO,"OP 991","hash-b");
  var opA=await Assess("operation-a");var opB=await Assess("operation-b");
  var approvals=await Task.WhenAll(Approve("operation-a",opA.GetProperty("reviewToken").GetString(),"Transferencia verificada por administrador"),Approve("operation-b",opB.GetProperty("reviewToken").GetString(),"Transferencia verificada por administrador"));
  Check(approvals.Count(r=>r.IsSuccessStatusCode)==1&&approvals.Count(r=>r.StatusCode==HttpStatusCode.Conflict)==1,"Aprobaciones simultáneas con misma operación habilitan un solo producto, incluso con revisión manual");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.PaymentEvidences.CountAsync(p=>(p.Id=="operation-a"||p.Id=="operation-b")&&p.Status==PaymentStatus.APROBADO)==1,"Solo un pago queda aprobado");Check(await db.Orders.CountAsync(o=>(o.Id=="o-operation-a"||o.Id=="o-operation-b")&&o.Status==OrderStatus.APROBADO)==1,"Solo la orden ganadora queda habilitada");Check(await db.StudentNotifications.CountAsync(n=>n.EventKey=="payment:operation-a"||n.EventKey=="payment:operation-b")==1,"Carrera no duplica notificaciones ni efectos de aprobación");}
  await Add("voucher-a","fraud-a","fraud-advisory",ProductType.ASESORIA,"VOUCHER-A","same-hash");await Add("voucher-b","fraud-b","fraud-course-3",ProductType.CURSO,"VOUCHER-B","same-hash");var vA=await Assess("voucher-a");var vB=await Assess("voucher-b");var vouchers=await Task.WhenAll(Approve("voucher-a",vA.GetProperty("reviewToken").GetString(),"Comprobante revisado y contrastado"),Approve("voucher-b",vB.GetProperty("reviewToken").GetString(),"Comprobante revisado y contrastado"));Check(vouchers.Count(r=>r.IsSuccessStatusCode)==1&&vouchers.Count(r=>r.StatusCode==HttpStatusCode.Conflict)==1,"Mismo voucher con diferentes bancos u operaciones no habilita dos productos");
  await Add("partial-source","fraud-a","fraud-course-4",ProductType.CURSO,"PARTIAL-1","partial-source-hash","Titular coincidente",status:PaymentStatus.APROBADO);await Add("partial-target","fraud-b","fraud-course-5",ProductType.CURSO,"PARTIAL-2","partial-target-hash","Titular coincidente");
  Check((await Approve("partial-target")).StatusCode==HttpStatusCode.Conflict,"Coincidencia parcial impide aprobar sin revisión");var partial=await Assess("partial-target");Check(partial.GetProperty("exactDuplicates").GetArrayLength()==0&&partial.GetProperty("partialMatches").GetArrayLength()==1,"Revisión distingue coincidencias sospechosas de duplicados exactos");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check((await db.PaymentEvidences.FindAsync("partial-target"))!.Status==PaymentStatus.EN_REVISION&&!await db.Enrollments.AnyAsync(e=>e.OrderId=="o-partial-target"),"Coincidencia queda en revisión sin matrícula");}
  Check((await Approve("partial-target",partial.GetProperty("reviewToken").GetString(),"corto")).StatusCode==HttpStatusCode.Conflict,"No acepta justificación insuficiente");
  Check((await Approve("partial-target",partial.GetProperty("reviewToken").GetString(),"Banco confirmó una transferencia independiente")).IsSuccessStatusCode,"Administrador puede aprobar coincidencia parcial con justificación y evidencia vigente");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.AuditLogs.AnyAsync(x=>x.EntityId=="partial-target"&&x.Action=="PAYMENT_PARTIAL_MATCH_ACCEPTED"&&x.Details.Contains("Banco confirmó")),"Excepción parcial registra responsable y justificación en auditoría");}
  await Add("stale-target","fraud-a","fraud-course-6",ProductType.CURSO,"PARTIAL-3","stale-old-hash","Titular coincidente");var stale=await Assess("stale-target");using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var p=(await db.PaymentEvidences.FindAsync("stale-target"))!;p.VoucherFileHash="stale-new-hash";await db.SaveChangesAsync();}
  Check((await Approve("stale-target",stale.GetProperty("reviewToken").GetString(),"Justificación para comprobante anterior")).StatusCode==HttpStatusCode.Conflict,"Cambiar el voucher invalida la revisión anterior");
  var changed=await Assess("stale-target");using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var p=(await db.PaymentEvidences.FindAsync("partial-source"))!;p.VoucherFileUrl="replacement-comparison-voucher";await db.SaveChangesAsync();}
  Check((await Approve("stale-target",changed.GetProperty("reviewToken").GetString(),"Justificación anterior al cambio del comprobante coincidente")).StatusCode==HttpStatusCode.Conflict,"Cambiar un comprobante coincidente también invalida la revisión");
  var bytes=new byte[]{137,80,78,71,13,10,26,10,1,2,3};var pngHash=Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes));
  await Add("upload-source","fraud-a","fraud-course-9",ProductType.CURSO,"UPLOAD-SOURCE",pngHash.ToUpperInvariant(),status:PaymentStatus.APROBADO);await Add("upload-target","fraud-b","fraud-course-10",ProductType.CURSO,"UPLOAD-TARGET","old-upload-hash");
  using(var form=new MultipartFormDataContent()){form.Add(new ByteArrayContent(bytes),"file","comprobante.png");using var req=new HttpRequestMessage(HttpMethod.Post,"/api/orders/o-upload-target/voucher"){Content=form};req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",b);Check((await http.SendAsync(req)).StatusCode==HttpStatusCode.Conflict,"Subida rechaza voucher aprobado incluso si el hash anterior usa mayúsculas");}
  await Add("bank-other","fraud-a","fraud-course-7",ProductType.CURSO,"BANK-444","other-bank-hash",bank:"INTERBANK",status:PaymentStatus.APROBADO);await Add("bank-target","fraud-b","fraud-course-8",ProductType.CURSO,"BANK444","target-bank-hash");Check((await Assess("bank-target")).GetProperty("partialMatches").GetArrayLength()==1,"Mismo número en bancos diferentes requiere revisión y no se declara fraude automáticamente");
  var rejected=await Send(HttpMethod.Get,"/api/admin/payments/partial-target",a);Check(rejected.StatusCode==HttpStatusCode.Forbidden,"Alumno no puede consultar las coincidencias de otros pagos");
  var duplicate=await Send(HttpMethod.Post,"/api/orders/checkout",a,Checkout("fraud-a","fraud-course-9",1,"BANK444"));Check(!duplicate.IsSuccessStatusCode||!(await duplicate.Content.ReadAsStringAsync()).Contains("ORD-bank-other"),"Checkout no filtra la orden de otro usuario");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var gate=scope.ServiceProvider.GetRequiredService<IPaymentMutationGate>();try{await gate.ExecuteAsync<int>(()=>throw new InvalidOperationException("fixture"),CancellationToken.None);}catch(InvalidOperationException){}Check(await gate.ExecuteAsync(()=>Task.FromResult(1),CancellationToken.None)==1,"Error libera el bloqueo para la siguiente solicitud");}
  Console.WriteLine(checks+" comprobaciones antifraude superadas; sin transacciones bancarias reales.");
 }

 public static async Task CheckPostgresLock()
 {
  // Only advisory locks and SELECTs. Never writes financial tables or changes schema.
  var config=new ConfigurationManager();config.SetBasePath(Path.GetFullPath("src/ForensicEdu.WebApi"));config.AddJsonFile("appsettings.json").AddUserSecrets("ForensicEdu-Local-Development").AddJsonFile("appsettings.Local.json",optional:true).AddEnvironmentVariables();using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(40));await config.AddOpenBaoSecretsAsync(timeout.Token);config.AddEnvironmentVariables();var connection=config.GetConnectionString("DefaultConnection");
  await using var a=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);await using var b=new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
  async Task Probe(bool fail){var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var second=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var one=new PaymentMutationGate(a).ExecuteAsync(async()=>await new CoursePurchaseGate(a).ExecuteAsync("antifraud-probe",Guid.NewGuid().ToString(),async()=>{entered.SetResult();await release.Task.WaitAsync(timeout.Token);if(fail)throw new InvalidOperationException("fixture rollback");return true;},timeout.Token),timeout.Token);await Task.WhenAny(entered.Task,one);if(one.IsCompleted)await one;await entered.Task;var two=new PaymentMutationGate(b).ExecuteAsync(()=>{second.SetResult();return Task.FromResult(true);},timeout.Token);await Task.Delay(300,timeout.Token);var blocked=!second.Task.IsCompleted;release.SetResult();try{await one;}catch(InvalidOperationException)when(fail){}await two;if(!blocked||!second.Task.IsCompleted)throw new Exception("No serializó las conexiones PostgreSQL.");Console.WriteLine("OK: Bloqueo global PostgreSQL + curso; libera tras "+(fail?"rollback":"commit")+"; sin escribir tablas.");}
  await Probe(false);await Probe(true);
  var probe=Guid.NewGuid().ToString("N");await ForensicEdu.Application.Services.PaymentAntifraud.AssessAsync(a,new PaymentEvidence{Id=probe,BankName="PROBE-"+probe,OperationNumber=probe,OperationDate=DateTime.UtcNow,Amount=0.01m,PaymentFingerprint=probe},timeout.Token);Console.WriteLine("OK: Consulta de coincidencias traducida y ejecutada en PostgreSQL sin escribir datos.");
 }
}

sealed class AntifraudVoucherStorage:IVoucherStorage
{
 public Task<string> SaveAsync(byte[] bytes,string extension,CancellationToken ct)=>Task.FromResult("fixture-"+Guid.NewGuid().ToString("N")+extension);
 public Task<ForensicEdu.Application.Common.Results.DownloadContent?> ReadAsync(string name,CancellationToken ct)=>Task.FromResult<ForensicEdu.Application.Common.Results.DownloadContent?>(null);
}
