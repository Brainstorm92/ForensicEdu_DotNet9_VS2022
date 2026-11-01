using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace ForensicEdu.Application.Services;

public sealed class VideoAdministration(IApplicationDbContext db,ICurrentUser current,IPrivateVideoStorage storage,IVideoUploadTokens tokens,ILogger<VideoAdministration> logger):IVideoAdministration
{
 private bool Staff=>EndpointSupport.HasRole(current,"1","2");
 private Task<CourseModule?> Module(string course,string module,CancellationToken ct)=>db.CourseModules.FirstOrDefaultAsync(m=>m.Id==module&&m.CourseId==course,ct);
 private VideoUploadClaim? Claim(string course,string module,string token){var c=tokens.Read(token);return c!=null&&c.UserId==current.UserId&&c.CourseId==course&&c.ModuleId==module&&PrivateVideos.Key(c.Key)&&PrivateVideos.Key(c.StagingKey,true)?c:null;}
 private ApplicationResult Unavailable(Exception e){logger.LogWarning("Operación de video no completada: {Type}",e.GetType().Name);return ApplicationResult.Unavailable(new{message="No se pudo conectar con el almacenamiento de videos. Comprueba la configuración de RustFS e inténtalo nuevamente."});}
 public async Task<ApplicationResult> BeginAsync(string courseId,string moduleId,VideoUploadInput input,CancellationToken ct){
  if(!Staff)return ApplicationResult.Forbidden();var m=await Module(courseId,moduleId,ct);if(m==null)return ApplicationResult.NotFound();
  if(input==null||string.IsNullOrWhiteSpace(input.FileName)||input.FileName.Length>300||!input.FileName.EndsWith(".mp4",StringComparison.OrdinalIgnoreCase)||input.Size<16||input.Size>PrivateVideos.MaxBytes)return ApplicationResult.Invalid(new{message="Selecciona un video MP4 de hasta 2 GB, preparado en H.264 y audio AAC."});
  var id=Guid.NewGuid().ToString("N");var expires=DateTime.UtcNow.AddMinutes(30);var c=new VideoUploadClaim(current.UserId,courseId,moduleId,"video-uploads/"+id+".mp4","videos/"+id+".mp4",input.Size,PrivateVideos.Source(m),expires);
  try{var url=await storage.UploadUrlAsync(c.StagingKey,expires,ct);return ApplicationResult.Success(new{uploadUrl=url,token=tokens.Create(c),expiresAt=expires,contentType="video/mp4",maxBytes=PrivateVideos.MaxBytes});}catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}catch(Exception e){return Unavailable(e);}
 }
 public async Task<ApplicationResult> FinishAsync(string courseId,string moduleId,string token,CancellationToken ct){
  if(!Staff)return ApplicationResult.Forbidden();var c=Claim(courseId,moduleId,token);if(c==null)return ApplicationResult.Invalid(new{message="La autorización de subida venció o no corresponde al módulo. Inicia la subida nuevamente."});var m=await Module(courseId,moduleId,ct);if(m==null)return ApplicationResult.NotFound();
  if(m.StreamingId==c.Key)return await PreviewAsync(courseId,moduleId,ct);
  if(PrivateVideos.Source(m)!=c.PreviousSource)return ApplicationResult.Conflict(new{message="Otro cambio actualizó el video del módulo. Recarga antes de reemplazarlo."});
  var published=false;var saving=false;
  try{
   var probe=await storage.ProbeAsync(c.StagingKey,ct);if(probe==null)return ApplicationResult.Invalid(new{message="La subida no ha terminado. Vuelve a seleccionar el archivo."});
   if(probe.Size!=c.Size||!probe.IsMp4){await QuietDelete(c.StagingKey);return ApplicationResult.Invalid(new{message="El archivo recibido no coincide con el tamaño declarado o no es un MP4 válido."});}
   await storage.PublishAsync(c.StagingKey,c.Key,ct);published=true;
   m.StreamingId=c.Key;m.VideoStreamingUrl="";EndpointSupport.Audit(db,current,"PRIVATE_VIDEO_UPLOADED","CourseModule",m.Id,"Video MP4 privado adjuntado al módulo.");
   saving=true;await db.SaveChangesAsync(ct);await QuietDelete(c.StagingKey);
   // Playback signing is separate: a preview failure never deletes a committed video.
   return ApplicationResult.Success(new{m.Id,hasPrivateVideo=true,message="Video privado guardado. Ya puedes previsualizarlo."});
  }catch(DbUpdateConcurrencyException){if(published&&!saving)await QuietDelete(c.Key);return ApplicationResult.Conflict(new{message="El módulo cambió durante la subida. Recarga para revisar el video actual."});}
  catch(OperationCanceledException) when(ct.IsCancellationRequested){if(published&&!saving)await QuietDelete(c.Key);throw;}
  catch(Exception e){if(published&&!saving)await QuietDelete(c.Key);return Unavailable(e);}
 }
 public async Task<ApplicationResult> CancelAsync(string courseId,string moduleId,string token,CancellationToken ct){if(!Staff)return ApplicationResult.Forbidden();var c=Claim(courseId,moduleId,token);if(c==null)return ApplicationResult.Invalid();await QuietDelete(c.StagingKey);return ApplicationResult.NoContent();}
 public async Task<ApplicationResult> PreviewAsync(string courseId,string moduleId,CancellationToken ct){if(!Staff)return ApplicationResult.Forbidden();var m=await Module(courseId,moduleId,ct);if(m==null)return ApplicationResult.NotFound();if(!PrivateVideos.Key(m.StreamingId))return ApplicationResult.NotFound();try{return ApplicationResult.Success(await storage.PlaybackAsync(m.StreamingId,ct));}catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}catch(Exception e){return Unavailable(e);}}
 public async Task<ApplicationResult> RemoveAsync(string courseId,string moduleId,CancellationToken ct){if(!Staff)return ApplicationResult.Forbidden();var m=await Module(courseId,moduleId,ct);if(m==null)return ApplicationResult.NotFound();if(!PrivateVideos.Key(m.StreamingId))return ApplicationResult.NoContent();m.StreamingId="";EndpointSupport.Audit(db,current,"PRIVATE_VIDEO_DETACHED","CourseModule",m.Id,"Video privado desvinculado; objeto conservado para recuperación administrativa.");try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){return ApplicationResult.Conflict(new{message="El módulo cambió. Recarga antes de quitar el video."});}return ApplicationResult.NoContent();}
 private async Task QuietDelete(string key){try{using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(15));await storage.DeleteAsync(key,timeout.Token);}catch(Exception e){logger.LogWarning("Limpieza temporal de video pendiente: {Type}",e.GetType().Name);}}
}
