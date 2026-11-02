using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Contracts.AdvisoryCare;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;
public sealed class AdvisoryCareService(IApplicationDbContext db,ICurrentUser actor):IAdvisoryCare
{
 private bool Admin=>actor.IsInRole("1")||actor.IsInRole("2");
 private IQueryable<AdvisoryCase> Visible(){var q=db.AdvisoryCases.AsQueryable();return Admin?q:actor.IsInRole("7")?q.Where(c=>c.ConsultantId==actor.UserId):q.Where(c=>c.StudentId==actor.UserId);}
 private static bool Final(string status)=>status is "COMPLETADA" or "CANCELADA";
 private static string Label(string status)=>status switch {"PENDIENTE_ASIGNACION"=>"Pendiente de asignación","ASIGNADA"=>"Consultor asignado","PROGRAMADA"=>"Programada","EN_ATENCION"=>"En atención","COMPLETADA"=>"Completada","REALIZADA"=>"Realizada","CANCELADA"=>"Cancelada",_=>status};
 private static AdvisoryCareException Invalid(string message)=>new(CareFailure.Invalid,message);
 private static void Text(string? value,int length,string label){if(value==null||value.Length>length)throw Invalid(label+" supera la longitud permitida.");}
 private static void Utc(DateTime value){if(value==default||value.Kind!=DateTimeKind.Utc)throw Invalid("Indique una fecha válida en UTC.");}
 private IQueryable<AdvisoryCase> Related(IQueryable<AdvisoryCase> q)=>q.Include(c=>c.Order).Include(c=>c.AdvisoryService).Include(c=>c.Student).Include(c=>c.Consultant).Include(c=>c.Sessions);
 private CareSummary Summary(AdvisoryCase c)=>new(c.Id,c.OrderId,c.Order.OrderCode,c.AdvisoryService.Title,c.StudentId,c.Student.FullName,c.Student.Email,c.Student.Phone,c.ConsultantId,c.Consultant?.FullName,c.Consultant?.Phone,c.Status,c.ScheduledAt,c.Sessions.Count,c.UpdatedAt,c.Version);
 public async Task<PageResult<CareSummary>> ListAsync(int page,int pageSize,string? search,string? status,CancellationToken ct){
  if(page<1||pageSize<1||pageSize>100)throw Invalid("Revise la paginación.");
  var q=Visible().AsNoTracking();
  if(!string.IsNullOrWhiteSpace(search)){if(search.Length>200)throw Invalid("La búsqueda es demasiado larga.");var term=search.Trim().ToLower();q=q.Where(c=>c.Student.FullName.ToLower().Contains(term)||c.AdvisoryService.Title.ToLower().Contains(term)||c.Order.OrderCode.ToLower().Contains(term));}
  if(!string.IsNullOrEmpty(status))q=q.Where(c=>c.Status==status);
  var total=await q.CountAsync(ct);var rows=await Related(q).OrderByDescending(c=>c.CreatedAt).ThenBy(c=>c.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
  return new(rows.Select(Summary).ToList(),total,page,pageSize);
 }
 public async Task<CareDetail> GetAsync(string id,CancellationToken ct){
  var c=await Related(Visible().AsNoTracking()).Include(c=>c.History).SingleOrDefaultAsync(c=>c.Id==id,ct)??throw new AdvisoryCareException(CareFailure.NotFound,"Atención no encontrada o sin acceso.");
  var manage=Admin||(actor.IsInRole("7")&&c.ConsultantId==actor.UserId);
  var actorIds=c.History.Select(h=>h.ActorId).Distinct().ToList();
  var names=manage?await db.Users.AsNoTracking().Where(u=>actorIds.Contains(u.Id)).ToDictionaryAsync(u=>u.Id,u=>u.FullName,ct):new Dictionary<string,string>();
  return new(Summary(c),c.Summary,manage?c.InternalNotes:null,manage&&!Final(c.Status),Admin&&!Final(c.Status),c.Sessions.OrderBy(s=>s.ScheduledAt).Select(s=>new CareSessionDto(s.Id,s.ScheduledAt,s.DurationMinutes,s.Status,s.Summary,manage?s.InternalNotes:null)).ToList(),c.History.OrderByDescending(h=>h.CreatedAt).ThenBy(h=>h.Id).Select(h=>new CareHistoryDto(h.CreatedAt,h.Action,h.Details,h.ActorId=="system"?"Sistema":manage?names.GetValueOrDefault(h.ActorId,"Equipo académico"):h.ActorId==c.ConsultantId?"Consultor":"Equipo académico")).ToList());
 }
 public async Task<List<ConsultantOption>> ConsultantsAsync(CancellationToken ct){
  if(!Admin)throw new AdvisoryCareException(CareFailure.Forbidden,"Solo administración puede asignar consultores.");
  var now=DateTime.UtcNow;
  return await db.Users.AsNoTracking().Where(u=>u.Role==UserRole.CONSULTOR&&u.Status!=AccountStatus.SUSPENDIDO&&u.Status!=AccountStatus.ELIMINADO&&u.Status!=AccountStatus.PENDIENTE_DE_ELIMINACION&&(!u.ValidUntil.HasValue||u.ValidUntil>now)).OrderBy(u=>u.FullName).Select(u=>new ConsultantOption(u.Id,u.FullName,u.Email)).ToListAsync(ct);
 }
 private async Task<AdvisoryCase> Editable(string id,string version,CancellationToken ct){
  var c=await Visible().Include(c=>c.Order).Include(c=>c.Sessions).SingleOrDefaultAsync(c=>c.Id==id,ct)??throw new AdvisoryCareException(CareFailure.NotFound,"Atención no encontrada o sin acceso.");
  if(!Admin&&!(actor.IsInRole("7")&&c.ConsultantId==actor.UserId))throw new AdvisoryCareException(CareFailure.Forbidden,"No puede modificar esta atención.");
  if(c.Order.Status!=OrderStatus.APROBADO)throw Invalid("La orden debe tener el pago aprobado.");
  if(Final(c.Status))throw new AdvisoryCareException(CareFailure.Conflict,"Esta atención está cerrada; su historial se conserva.");
  if(string.IsNullOrEmpty(version)||version!=c.Version)throw new AdvisoryCareException(CareFailure.Conflict,"La atención cambió. Actualice los datos antes de guardar.");
  return c;
 }
 private void History(AdvisoryCase c,string action,string detail){c.UpdatedAt=DateTime.UtcNow;c.Version=Guid.NewGuid().ToString();db.AdvisoryCareHistory.Add(new(){AdvisoryCaseId=c.Id,ActorId=actor.UserId,Action=action,Details=detail});db.AuditLogs.Add(new(){UserId=actor.UserId,Action=action,EntityType="AdvisoryCase",EntityId=c.Id,Details=detail});}
 private async Task Commit(CancellationToken ct){try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){throw new AdvisoryCareException(CareFailure.Conflict,"Otra persona modificó la atención. Actualice los datos y vuelva a intentar.");}}
 public async Task AssignAsync(string id,CareAssignment input,CancellationToken ct){
  if(!Admin)throw new AdvisoryCareException(CareFailure.Forbidden,"Solo administración puede asignar consultores.");
  var c=await Editable(id,input.Version,ct);var consultant=(await ConsultantsAsync(ct)).SingleOrDefault(x=>x.Id==input.ConsultantId)??throw Invalid("Seleccione un consultor activo y con acceso vigente.");
  if(c.ConsultantId==consultant.Id)return;
  var previous=c.ConsultantId;c.ConsultantId=consultant.Id;if(c.Status=="PENDIENTE_ASIGNACION")c.Status="ASIGNADA";
  History(c,"ADVISORY_CONSULTANT_ASSIGNED",(previous==null?"Consultor asignado: ":"Consultor reasignado: ")+consultant.Name+".");await Commit(ct);
 }
 public async Task UpdateAsync(string id,CareUpdate input,CancellationToken ct){
  var c=await Editable(id,input.Version,ct);
  if(input.Status is not("PENDIENTE_ASIGNACION" or "ASIGNADA" or "PROGRAMADA" or "EN_ATENCION" or "COMPLETADA" or "CANCELADA"))throw Invalid("Estado de atención inválido.");
  Text(input.Summary,5000,"El resumen");Text(input.InternalNotes,10000,"Las observaciones");if(input.ScheduledAt.HasValue)Utc(input.ScheduledAt.Value);
  if(c.ConsultantId==null&&input.Status is not("PENDIENTE_ASIGNACION" or "CANCELADA"))throw Invalid("Primero asigne un consultor.");
  if(c.ConsultantId!=null&&input.Status=="PENDIENTE_ASIGNACION")throw Invalid("La atención ya tiene un consultor asignado.");
  if(input.Status=="PROGRAMADA"&&!input.ScheduledAt.HasValue)throw Invalid("Indique la fecha de atención.");
  if(input.Status=="COMPLETADA"&&!c.Sessions.Any(s=>s.Status=="REALIZADA"))throw Invalid("Registre al menos una sesión realizada antes de completar la atención.");
  if(input.Status=="CANCELADA"&&string.IsNullOrWhiteSpace(input.Summary))throw Invalid("Indique el motivo de cancelación en el resumen compartido.");
  if(Final(input.Status)&&c.Sessions.Any(s=>s.Status=="PROGRAMADA"))throw Invalid("Actualice o cancele las sesiones programadas antes de cerrar la atención.");
  var old=c.Status;c.Status=input.Status;c.ScheduledAt=input.ScheduledAt;c.Summary=input.Summary.Trim();c.InternalNotes=input.InternalNotes.Trim();History(c,"ADVISORY_CARE_UPDATED",old==c.Status?"Datos de atención actualizados.":"Estado: "+Label(old)+" → "+Label(c.Status)+".");await Commit(ct);
 }
 public async Task SaveSessionAsync(string id,string? sessionId,CareSessionInput input,CancellationToken ct){
  var c=await Editable(id,input.Version,ct);if(c.ConsultantId==null)throw Invalid("Primero asigne un consultor.");Utc(input.ScheduledAt);Text(input.Summary,5000,"El resumen");Text(input.InternalNotes,10000,"Las observaciones");
  if(input.DurationMinutes<1||input.DurationMinutes>480||input.Status is not("PROGRAMADA" or "REALIZADA" or "CANCELADA"))throw Invalid("Revise la duración y el estado de la sesión.");
  if(input.Status=="REALIZADA"&&(input.ScheduledAt>DateTime.UtcNow||string.IsNullOrWhiteSpace(input.Summary)))throw Invalid("Una sesión realizada necesita fecha pasada y resumen de atención.");
  var session=sessionId==null?new AdvisorySession{AdvisoryCaseId=c.Id}:c.Sessions.SingleOrDefault(s=>s.Id==sessionId)??throw new AdvisoryCareException(CareFailure.NotFound,"Sesión no encontrada en esta atención.");
  if(sessionId==null)db.AdvisorySessions.Add(session);
  session.ScheduledAt=input.ScheduledAt;session.DurationMinutes=input.DurationMinutes;session.Status=input.Status;session.Summary=input.Summary.Trim();session.InternalNotes=input.InternalNotes.Trim();session.UpdatedAt=DateTime.UtcNow;
  if(input.Status=="REALIZADA")c.Status="EN_ATENCION";else if(input.Status=="PROGRAMADA"&&c.Status=="ASIGNADA")c.Status="PROGRAMADA";
  if(input.Status=="PROGRAMADA"&&(!c.ScheduledAt.HasValue||input.ScheduledAt<c.ScheduledAt))c.ScheduledAt=input.ScheduledAt;
  History(c,sessionId==null?"ADVISORY_SESSION_CREATED":"ADVISORY_SESSION_UPDATED","Sesión "+Label(input.Status)+" · "+input.ScheduledAt.AddHours(-5).ToString("dd/MM/yyyy HH:mm 'Lima'")+" · "+input.DurationMinutes+" min.");await Commit(ct);
 }
}
