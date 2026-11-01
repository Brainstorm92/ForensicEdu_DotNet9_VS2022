using System.Text.Json;
using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetModuleVideo;
public sealed class GetModuleVideoQueryHandler(IApplicationDbContext db,ICurrentUser current,IPrivateVideoStorage storage):IRequestHandler<GetModuleVideoQuery,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(GetModuleVideoQuery request,CancellationToken ct){
  var e=await db.Enrollments.AsNoTracking().Include(x=>x.User).Include(x=>x.Course).ThenInclude(x=>x!.Modules).FirstOrDefaultAsync(x=>x.Id==request.Id&&x.UserId==current.UserId,ct);
  if(e?.Course==null||e.User==null)return ApplicationResult.NotFound();
  if((e.Status!="ACTIVA"&&e.Status!="COMPLETADA")||e.User.ValidUntil<=DateTime.UtcNow||e.User.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION or AccountStatus.VENCIDA)return ApplicationResult.Forbidden();
  if(!await db.Orders.AnyAsync(o=>o.Id==e.OrderId&&o.UserId==current.UserId&&o.Status==OrderStatus.APROBADO,ct))return ApplicationResult.Forbidden();
  var modules=e.Course.Modules.OrderBy(m=>m.OrderIndex).ToList();var module=modules.FirstOrDefault(m=>m.Id==request.ModuleId);if(module==null)return ApplicationResult.NotFound();
  var completed=JsonSerializer.Deserialize<HashSet<string>>(e.CompletedModulesJson)??[];
  if(modules.TakeWhile(m=>m.Id!=module.Id).Any(m=>!completed.Contains(m.Id)))return ApplicationResult.Conflict(new{message="Completa los módulos anteriores para abrir este video."});
  if(PrivateVideos.Key(module.StreamingId)){try{return ApplicationResult.Success(await storage.PlaybackAsync(module.StreamingId,ct));}catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}catch{return ApplicationResult.Unavailable(new{message="El video privado no está disponible temporalmente. Inténtalo nuevamente."});}}
  if(string.IsNullOrWhiteSpace(module.VideoStreamingUrl))return ApplicationResult.Unavailable(new{message="El video está pendiente de configuración."});
  return ApplicationResult.Success(new{url=module.VideoStreamingUrl,privateVideo=false});
 }
}
