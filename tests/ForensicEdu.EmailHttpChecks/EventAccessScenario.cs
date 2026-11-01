using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

static class EventAccessScenario
{
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens)
 {
  var count=0;void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  using(var scope=services.CreateScope()){
   var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   foreach(var state in new[]{"live","early","cancelled","finished","missing-room","unpaid"}){
    db.LiveEvents.Add(new LiveEvent{Id=state,Title=state,EventDate=state=="early"?DateTime.UtcNow.AddHours(1):DateTime.UtcNow.AddMinutes(-2),AccessStatus=state=="cancelled"?"CANCELADO":state=="finished"?"FINALIZADO":"EN_VIVO",RoomUrl=state=="missing-room"?"":"https://meet.google.com/fixture-only"});
    db.Orders.Add(new Order{Id="order-"+state,UserId="student",ProductId=state,ProductType=ProductType.EVENTO,Status=state=="unpaid"?OrderStatus.PENDIENTE_VALIDACION:OrderStatus.APROBADO});
   }await db.SaveChangesAsync();
  }
  Check((await http.GetAsync("/api/student/events/live/access")).StatusCode==HttpStatusCode.Unauthorized,"Sala requiere sesión autenticada");
  http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id="student",Role=(UserRole)6}));
  var response=await http.GetAsync("/api/student/events/live/access");var value=await response.Content.ReadFromJsonAsync<JsonElement>();
  Check(response.IsSuccessStatusCode&&value.GetProperty("url").GetString()=="https://meet.google.com/fixture-only","Pago aprobado entrega exactamente el enlace configurado");
  Check((await http.GetAsync("/api/student/events/early/access")).StatusCode==HttpStatusCode.Conflict,"No se permite acceso antes de la ventana de 15 minutos");
  Check((await http.GetAsync("/api/student/events/cancelled/access")).StatusCode==HttpStatusCode.Conflict,"Evento cancelado no entrega el enlace");
  Check((await http.GetAsync("/api/student/events/finished/access")).StatusCode==HttpStatusCode.Conflict,"Evento finalizado no entrega el enlace");
  Check((await http.GetAsync("/api/student/events/missing-room/access")).StatusCode==HttpStatusCode.ServiceUnavailable,"Sala sin configurar informa indisponibilidad");
  Check((await http.GetAsync("/api/student/events/unpaid/access")).StatusCode==HttpStatusCode.Forbidden,"Pago pendiente no habilita sala");
  foreach(var invalidLink in new[]{"   ","http://example.invalid/room","javascript:alert(1)","not-a-url","https://user:password@example.invalid/room"}){
   using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();var room=await db.LiveEvents.FindAsync("live");room!.RoomUrl=invalidLink;await db.SaveChangesAsync();}
   var rejected=await http.GetAsync("/api/student/events/live/access");
   Check(rejected.StatusCode==HttpStatusCode.ServiceUnavailable,"Enlace vacío o inseguro se rechaza sin devolver URL: "+(invalidLink.Trim().Length==0?"vacío":invalidLink.Split(':')[0]));
  }
  http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id="other-student",Role=(UserRole)6}));
  Check((await http.GetAsync("/api/student/events/live/access")).StatusCode==HttpStatusCode.Forbidden,"Otro alumno no utiliza una inscripción ajena");
  Console.WriteLine($"{count} comprobaciones de acceso a eventos superadas sin abrir reuniones reales.");
 }
}
