using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Application.Contracts.AdvisoryCare;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class AdvisoryCareScenario
{
 public static async Task Seed(IServiceProvider services){using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  db.Users.AddRange(new User{Id="admin-fixture",FullName="Administración de prueba",Role=UserRole.SUPERADMIN,Email="admin@example.invalid"},new User{Id="consultant-1",FullName="Consultor de balística",Role=UserRole.CONSULTOR,Email="consultant1@example.invalid",Phone="+51999999999",ValidUntil=null},new User{Id="consultant-2",FullName="Consultora de criminalística",Role=UserRole.CONSULTOR,Email="consultant2@example.invalid",ValidUntil=null},new User{Id="other-student",FullName="Otro alumno",Email="other@example.invalid"});
  var user=await db.Users.SingleAsync(u=>u.Id=="student");user.FullName="Alumno de prueba";user.Phone="+51988888888";
  db.AdvisoryServices.Add(new AdvisoryService{Id="care-product",Title="Asesoría de peritaje judicial",Specialty="Balística",Specialist="Equipo de consultores",Price=250});
  for(var i=1;i<=2;i++){var order=new Order{Id="care-order-"+i,OrderCode="ORD-CARE-000"+i,UserId="student",ProductId="care-product",ProductTitle="Asesoría de peritaje judicial",ProductType=ProductType.ASESORIA,Amount=250,Price=250};db.Orders.Add(order);db.PaymentEvidences.Add(new PaymentEvidence{Id="care-payment-"+i,OrderId=order.Id,Amount=250,BankName="BANK",OperationNumber="CARE-OP-"+i,PaymentFingerprint="care-fingerprint-"+i,VoucherFileHash="care-voucher-"+i});}
  await db.SaveChangesAsync();
 }
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens){
  await Seed(services);var count=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  void As(string id,int role){http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id=id,Role=(UserRole)role}));}
  async Task<CareDetail> Get(string id)=>await http.GetFromJsonAsync<CareDetail>("/api/advisory-care/"+id)??throw new Exception("Missing care");
  Check((await http.GetAsync("/api/advisory-care")).StatusCode==HttpStatusCode.Unauthorized,"Atenciones requieren autenticación");
  As("admin-fixture",1);Check((await http.PostAsync("/api/admin/payments/care-payment-1/approve",null)).IsSuccessStatusCode,"Aprobar compra crea expediente de atención en la misma operación");
  string id;using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var c=await db.AdvisoryCases.SingleAsync();id=c.Id;Check(c.OrderId=="care-order-1"&&c.Status=="PENDIENTE_ASIGNACION"&&await db.AdvisoryCareHistory.CountAsync()==1,"Expediente vinculado a orden y con historial inicial");}
  await http.PostAsync("/api/admin/payments/care-payment-1/approve",null);using(var scope=services.CreateScope()){Check(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().AdvisoryCases.CountAsync()==1,"Repetir aprobación no duplica expediente");}
  await http.PostAsync("/api/admin/payments/care-payment-2/approve",null);string second;using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();second=(await db.AdvisoryCases.SingleAsync(c=>c.OrderId=="care-order-2")).Id;Check(await db.AdvisoryCases.CountAsync()==2,"Dos compras del mismo servicio tienen expedientes independientes");}
  As("consultant-1",7);var before=await http.GetFromJsonAsync<JsonElement>("/api/advisory-care");Check(before.GetProperty("total").GetInt32()==0,"Consultor no ve compras sin asignar");Check((await http.GetAsync("/api/advisory-care/"+id)).StatusCode==HttpStatusCode.NotFound,"Consultor no accede a expediente ajeno por ID");
  As("student",6);Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("ASIGNADA",null,"","","x"))).StatusCode==HttpStatusCode.Forbidden,"Alumno no modifica la atención");
  As("admin-fixture",1);var detail=await Get(id);Check((await http.PutAsJsonAsync("/api/advisory-care/"+id+"/assignment",new CareAssignment("student",detail.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"No permite asignar una cuenta que no sea consultor activo");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id+"/assignment",new CareAssignment("consultant-1",detail.Case.Version))).IsSuccessStatusCode,"Administración asigna consultor por compra");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("ASIGNADA",null,"","",detail.Case.Version))).StatusCode==HttpStatusCode.Conflict,"Versión anterior no sobrescribe una asignación nueva");
  As("consultant-2",7);Check((await http.GetAsync("/api/advisory-care/"+id)).StatusCode==HttpStatusCode.NotFound,"Otro consultor no puede leer la atención");
  As("consultant-1",7);detail=await Get(id);Check(detail.CanManage&&!detail.CanAssign,"Consultor asignado gestiona sesiones pero no reasigna");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id+"/assignment",new CareAssignment("consultant-2",detail.Case.Version))).StatusCode==HttpStatusCode.Forbidden,"Consultor no puede reasignarse a otra persona");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("COMPLETADA",null,"","",detail.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"No se completa una atención sin sesión realizada");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("PROGRAMADA",null,"","",detail.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"Estado programado exige fecha de atención");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("PROGRAMADA",DateTime.UtcNow.AddDays(1),"Resumen para alumno","SECRETO_INTERNO_CASO",detail.Case.Version))).IsSuccessStatusCode,"Consultor registra fecha, estado y notas privadas");
  detail=await Get(id);Check((await http.PostAsJsonAsync("/api/advisory-care/"+id+"/sessions",new CareSessionInput(DateTime.UtcNow.AddDays(1),60,"PROGRAMADA","Primera coordinación","SECRETO_INTERNO_SESION",detail.Case.Version))).IsSuccessStatusCode,"Sesión programada queda vinculada a su compra");
  detail=await Get(id);var sid=detail.Sessions.Single().Id;
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("CANCELADA",null,"Motivo compartido","",detail.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"No cierra atención con sesiones pendientes");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id+"/sessions/"+sid,new CareSessionInput(DateTime.UtcNow.AddDays(1),60,"REALIZADA","Resumen","",detail.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"Sesión futura no puede marcarse realizada");
  Check((await http.PutAsJsonAsync("/api/advisory-care/"+id+"/sessions/"+sid,new CareSessionInput(DateTime.UtcNow.AddHours(-1),60,"REALIZADA","Se revisó el informe pericial.","SECRETO_INTERNO_SESION",detail.Case.Version))).IsSuccessStatusCode,"Consultor registra sesión realizada con resumen");
  As("student",6);var studentResponse=await http.GetAsync("/api/advisory-care/"+id);var json=await studentResponse.Content.ReadAsStringAsync();Check(studentResponse.IsSuccessStatusCode&&!json.Contains("SECRETO_INTERNO")&&!json.Contains("internalNotes"),"Notas internas del caso y sesiones nunca se devuelven al alumno");var student=await Get(id);Check(!student.CanManage&&!student.CanAssign&&student.Case.ConsultantName=="Consultor de balística"&&student.Sessions.Count==1,"Alumno ve consultor, sesión e historial propios en lectura");
  As("other-student",6);Check((await http.GetAsync("/api/advisory-care/"+id)).StatusCode==HttpStatusCode.NotFound,"Alumno no consulta asesoría de otra cuenta");
  As("consultant-1",7);detail=await Get(id);Check((await http.PutAsJsonAsync("/api/advisory-care/"+id,new CareUpdate("COMPLETADA",detail.Case.ScheduledAt,"Atención completada","SECRETO_INTERNO_CASO",detail.Case.Version))).IsSuccessStatusCode,"Atención puede completarse después de una sesión realizada");detail=await Get(id);Check(!detail.CanManage&&detail.History.Count>=5,"Atención cerrada conserva historial y queda en lectura");
  Check((await http.PostAsJsonAsync("/api/advisory-care/"+id+"/sessions",new CareSessionInput(DateTime.UtcNow,30,"REALIZADA","Otra","",detail.Case.Version))).StatusCode==HttpStatusCode.Conflict,"No se agregan sesiones a atención cerrada");
  As("admin-fixture",1);var next=await Get(second);await http.PutAsJsonAsync("/api/advisory-care/"+second+"/assignment",new CareAssignment("consultant-1",next.Case.Version));next=await Get(second);await http.PutAsJsonAsync("/api/advisory-care/"+second+"/assignment",new CareAssignment("consultant-2",next.Case.Version));As("consultant-1",7);Check((await http.GetAsync("/api/advisory-care/"+second)).StatusCode==HttpStatusCode.NotFound,"Consultor anterior pierde acceso tras reasignación");
  As("consultant-2",7);next=await Get(second);Check((await http.PutAsJsonAsync("/api/advisory-care/"+second,new CareUpdate("CANCELADA",null,"","",next.Case.Version))).StatusCode==HttpStatusCode.BadRequest,"Cancelar exige un motivo compartido");Check((await http.PutAsJsonAsync("/api/advisory-care/"+second,new CareUpdate("CANCELADA",null,"Cancelación coordinada con el alumno.","",next.Case.Version))).IsSuccessStatusCode,"Cancelación queda registrada sin eliminar historial");
  using(var first=services.CreateScope())using(var other=services.CreateScope()){var a=first.ServiceProvider.GetRequiredService<ApplicationDbContext>();var b=other.ServiceProvider.GetRequiredService<ApplicationDbContext>();var x=await a.AdvisoryCases.FindAsync(id);var y=await b.AdvisoryCases.FindAsync(id);x!.Version=Guid.NewGuid().ToString();y!.Version=Guid.NewGuid().ToString();await a.SaveChangesAsync();try{await b.SaveChangesAsync();throw new Exception("Concurrent save allowed");}catch(DbUpdateConcurrencyException){Check(true,"Concurrencia EF bloquea sobrescritura simultánea");}}
  Console.WriteLine($"{count} comprobaciones de atención de asesorías superadas sin PostgreSQL remoto.");
 }
}
