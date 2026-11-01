using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

static class CatalogScenario
{
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService tokens)
 {
  var count=0;void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("OK: "+label);count++;}
  Check((await http.GetAsync("/api/admin/events")).StatusCode==HttpStatusCode.Unauthorized,"Eventos requieren una sesión");
  http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id="finance",Role=(UserRole)3}));
  Check((await http.GetAsync("/api/admin/advisories")).StatusCode==HttpStatusCode.Forbidden,"Administrador financiero no gestiona asesorías");
  http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",tokens.Create(new User{Id="academic",Role=(UserRole)2}));
  var input=new EventInput("Seminario de prueba","Seminario","Descripción",45.50m,new DateTime(2027,1,15,14,30,0,DateTimeKind.Utc),"09:30 - 12:00 (Lima)","Ponente de prueba","Biografía","https://example.invalid/sala","https://example.invalid/banner.jpg");
  var created=await http.PostAsJsonAsync("/api/admin/events",input);Check(created.IsSuccessStatusCode,"Administrador académico crea evento mediante API");
  var id=(await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;
  var row=await http.GetFromJsonAsync<JsonElement>("/api/admin/events/"+id);
  Check(row.GetProperty("eventDate").GetDateTime()==input.EventDate&&row.GetProperty("roomUrl").GetString()==input.RoomUrl,"Fecha UTC y sala privada se conservan al editar");
  var invalid=await http.PostAsJsonAsync("/api/admin/events",input with {RoomUrl="http://insecure.invalid"});Check(invalid.StatusCode==HttpStatusCode.BadRequest,"Sala sin HTTPS rechazada por validación real");
  var updated=await http.PutAsJsonAsync("/api/admin/events/"+id,input with {Title="Seminario actualizado",Price=75,AccessStatus="EN_VIVO"});Check(updated.IsSuccessStatusCode,"Evento admite cambios de precio y estado");
  var listing=await http.GetFromJsonAsync<JsonElement>("/api/admin/events?search=actualizado&page=1&pageSize=20");Check(listing.GetProperty("total").GetInt32()==1,"Búsqueda administrativa devuelve evento actualizado");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Orders.Add(new Order{Id="event-order",UserId="student",ProductId=id,ProductType=ProductType.EVENTO});await db.SaveChangesAsync();}
  Check((await http.DeleteAsync("/api/admin/events/"+id)).StatusCode==HttpStatusCode.NoContent,"Cancelación del evento funciona con órdenes existentes");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check((await db.LiveEvents.FindAsync(id))?.AccessStatus=="CANCELADO"&&await db.Orders.AnyAsync(o=>o.Id=="event-order"),"Cancelar conserva el evento y la orden");}
  var advice=new AdvisoryInput("Asesoría pericial de prueba","Especialista de prueba","Balística","Consulta individual",120,"+51999999999");
  var response=await http.PostAsJsonAsync("/api/admin/advisories",advice);Check(response.IsSuccessStatusCode,"Asesoría se crea mediante API");
  var aid=(await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;
  Check((await http.PutAsJsonAsync("/api/admin/advisories/"+aid,advice with {Specialist="Especialista actualizado",Price=150})).IsSuccessStatusCode,"Asesoría permite actualizar especialista y tarifa");
  var advisory=await http.GetFromJsonAsync<JsonElement>("/api/admin/advisories/"+aid);Check(advisory.GetProperty("specialist").GetString()=="Especialista actualizado"&&advisory.GetProperty("price").GetDecimal()==150,"Datos editados persisten y se recuperan");
  Check((await http.PostAsJsonAsync("/api/admin/advisories",advice with {WhatsappContact="abc"})).StatusCode==HttpStatusCode.BadRequest,"Contacto WhatsApp inválido rechazado");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();db.Orders.Add(new Order{Id="advisory-order",UserId="student",ProductId=aid,ProductType=ProductType.ASESORIA});await db.SaveChangesAsync();}
  Check((await http.DeleteAsync("/api/admin/advisories/"+aid)).StatusCode==HttpStatusCode.Conflict,"Asesoría con órdenes no puede eliminarse");
  var fresh=await http.PostAsJsonAsync("/api/admin/advisories",advice with {Title="Sin órdenes"});var emptyId=(await fresh.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString()!;
  Check((await http.DeleteAsync("/api/admin/advisories/"+emptyId)).StatusCode==HttpStatusCode.NoContent,"Asesoría sin órdenes puede eliminarse");
  Check((await http.GetAsync("/api/admin/advisories/"+emptyId)).StatusCode==HttpStatusCode.NotFound,"Listado de detalle ya no devuelve la asesoría eliminada");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check(await db.AuditLogs.AnyAsync(a=>a.Action=="EVENT_SAVED")&&await db.AuditLogs.AnyAsync(a=>a.Action=="ADVISORY_REMOVED"),"Gestión de eventos y asesorías queda auditada");}
  Console.WriteLine($"{count} comprobaciones de eventos y asesorías superadas con datos en memoria.");
 }
}
