using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

sealed class FakeVideoStorage:IPrivateVideoStorage
{
 public ConcurrentDictionary<string,byte[]> Objects=new();public bool Fail;public bool FailPreview=false;
 public Task<string> UploadUrlAsync(string key,DateTime expires,CancellationToken ct){if(Fail)throw new IOException();return Task.FromResult("https://storage.example.invalid/"+key+"?fixture=upload");}
 public Task<VideoProbe?> ProbeAsync(string key,CancellationToken ct)=>Task.FromResult(Objects.TryGetValue(key,out var bytes)?new VideoProbe(bytes.Length,bytes.Length>=12&&System.Text.Encoding.ASCII.GetString(bytes,4,4)=="ftyp"):null);
 public Task PublishAsync(string source,string key,CancellationToken ct){if(Fail)throw new IOException();Objects[key]=Objects[source].ToArray();return Task.CompletedTask;}
 public Task<VideoPlayback> PlaybackAsync(string key,CancellationToken ct){if(FailPreview)throw new IOException();return Task.FromResult(new VideoPlayback("https://storage.example.invalid/"+key+"?fixture=private",DateTime.UtcNow.AddMinutes(15)));}
 public Task DeleteAsync(string key,CancellationToken ct){Objects.TryRemove(key,out _);return Task.CompletedTask;}
 public Task CleanStagingAsync(CancellationToken ct)=>Task.CompletedTask;
}
static class VideoScenario
{
 public static void MapStorage(WebApplication app){} // No public upload endpoints or real objects in tests.
 public static async Task Seed(IServiceProvider services){using var scope=services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  if(!await db.Users.AnyAsync(u=>u.Id=="admin-fixture"))db.Users.Add(new User{Id="admin-fixture",Email="admin@example.invalid",Role=UserRole.SUPERADMIN});
  db.Courses.Add(new Course{Id="video-course",Title="Curso de prueba privada",IsActive=true,IsPublished=false});
  db.CourseModules.AddRange(new CourseModule{Id="video-module",CourseId="video-course",Title="Primer módulo",OrderIndex=0},new CourseModule{Id="video-module-2",CourseId="video-course",Title="Segundo módulo",OrderIndex=1});
  db.Orders.Add(new Order{Id="video-order",OrderCode="VIDEO-ORDER",UserId="student",ProductType=ProductType.CURSO,ProductId="video-course",Status=OrderStatus.APROBADO});
  db.Enrollments.Add(new Enrollment{Id="video-enrollment",UserId="student",CourseId="video-course",OrderId="video-order"});await db.SaveChangesAsync();
 }
 public static async Task Run(HttpClient http,IServiceProvider services,TokenService auth){
  await Seed(services);int count=0;void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("OK: "+message);count++;}
  void As(string id,int role)=>http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",auth.Create(new User{Id=id,Role=(UserRole)role}));
  const string path="/api/admin/courses/video-course/modules/video-module/video";
  var memory=services.GetRequiredService<FakeVideoStorage>();byte[] bytes=new byte[64];System.Text.Encoding.ASCII.GetBytes("ftypisom").CopyTo(bytes,4);
  Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=64})).StatusCode==HttpStatusCode.Unauthorized,"Carga requiere autenticación");
  As("student",6);Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=64})).StatusCode==HttpStatusCode.Forbidden,"Alumno no puede cargar videos");
  As("finance",3);Check((await http.DeleteAsync(path)).StatusCode==HttpStatusCode.Forbidden,"Rol financiero no modifica videos");
  As("admin-fixture",1);Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.exe",size=64})).StatusCode==HttpStatusCode.BadRequest,"Extensión no MP4 rechazada");
  Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=PrivateVideos.MaxBytes+1})).StatusCode==HttpStatusCode.BadRequest,"Tamaño superior a 2 GB rechazado antes de cargar");
  Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=2147483648L})).IsSuccessStatusCode,"Límite exacto de 2 GB admite autorización sin cargar el archivo");
  async Task<(string Token,VideoUploadClaim Claim)> Start(){var response=await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=64});if(!response.IsSuccessStatusCode)throw new Exception("Inicio de subida falló: "+(int)response.StatusCode);var payload=await response.Content.ReadFromJsonAsync<JsonElement>();var token=payload.GetProperty("token").GetString()!;using var scope=services.CreateScope();var claim=scope.ServiceProvider.GetRequiredService<IVideoUploadTokens>().Read(token)!;return(token,claim);}
  var upload=await Start();Check(upload.Claim.Size==64&&PrivateVideos.Key(upload.Claim.StagingKey,true),"Autorización vincula módulo, propietario, tamaño y ruta temporal");
  Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token})).StatusCode==HttpStatusCode.BadRequest,"No vincula un video cuya subida no terminó");
  Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token+"x"})).StatusCode==HttpStatusCode.BadRequest,"Token alterado rechazado");
  As("other-admin",2);Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token})).StatusCode==HttpStatusCode.BadRequest,"Otra cuenta no puede finalizar la subida");As("admin-fixture",1);
  memory.Objects[upload.Claim.StagingKey]=new byte[64];Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token})).StatusCode==HttpStatusCode.BadRequest,"Firma de archivo distinta de MP4 rechazada");
  memory.Objects[upload.Claim.StagingKey]=bytes;Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token})).IsSuccessStatusCode,"Video validado se vincula al módulo");
  Check(memory.Objects.ContainsKey(upload.Claim.Key)&&!memory.Objects.ContainsKey(upload.Claim.StagingKey),"Publicación privada crea objeto independiente y limpia el temporal");
  memory.Objects[upload.Claim.StagingKey]=new byte[64];Check(memory.Objects[upload.Claim.Key][4]==(byte)'f',"Reutilizar PUT temporal no modifica el video publicado");
  Check((await http.PostAsJsonAsync(path+"/complete",new{token=upload.Token})).IsSuccessStatusCode,"Finalización repetida es idempotente");
  var preview=await http.GetAsync(path);Check(preview.IsSuccessStatusCode&&preview.Headers.CacheControl?.NoStore==true,"Vista previa administrativa temporal sin caché");
  As("student",6);Check((await http.GetAsync(path)).StatusCode==HttpStatusCode.Forbidden,"Alumno no utiliza la vista previa administrativa");
  const string playback="/api/student/enrollments/video-enrollment/modules/video-module/video";
  var play=await http.GetAsync(playback);var info=await play.Content.ReadFromJsonAsync<JsonElement>();Check(play.IsSuccessStatusCode&&info.GetProperty("privateVideo").GetBoolean()&&info.TryGetProperty("expiresAt",out _)&&play.Headers.CacheControl?.NoStore==true,"Alumno matriculado recibe enlace temporal privado sin caché");
  var classroom=await http.GetAsync("/api/student/enrollments/video-enrollment/classroom");var classroomBody=await classroom.Content.ReadAsStringAsync();var classroomJson=JsonDocument.Parse(classroomBody).RootElement;
  Check(classroom.IsSuccessStatusCode&&classroomJson.GetProperty("modules")[0].GetProperty("hasPrivateVideo").GetBoolean()&&!classroomBody.Contains(upload.Claim.Key)&&!classroomBody.Contains("storage.example.invalid"),"Aula expone disponibilidad privada sin revelar objetos ni enlaces de reproducción");
  As("someone-else",6);Check((await http.GetAsync("/api/student/enrollments/video-enrollment/classroom")).StatusCode==HttpStatusCode.NotFound,"Aula ajena no expone portada ni programa privado");
  As("someone-else",6);Check((await http.GetAsync(playback)).StatusCode==HttpStatusCode.NotFound,"Matrícula ajena no expone URL");As("student",6);
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();(await db.CourseModules.FindAsync("video-module-2"))!.StreamingId=upload.Claim.Key;await db.SaveChangesAsync();}
  Check((await http.GetAsync("/api/student/enrollments/video-enrollment/modules/video-module-2/video")).StatusCode==HttpStatusCode.Conflict,"No salta módulos bloqueados");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();(await db.Users.FindAsync("student"))!.ValidUntil=DateTime.UtcNow.AddDays(-1);await db.SaveChangesAsync();}
  Check((await http.GetAsync(playback)).StatusCode==HttpStatusCode.Forbidden,"Cuenta vencida no renueva acceso al video");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();(await db.Users.FindAsync("student"))!.ValidUntil=DateTime.UtcNow.AddDays(30);(await db.Orders.FindAsync("video-order"))!.Status=OrderStatus.PENDIENTE_VALIDACION;await db.SaveChangesAsync();}
  Check((await http.GetAsync(playback)).StatusCode==HttpStatusCode.Forbidden,"Pago no aprobado impide reproducción privada");
  As("admin-fixture",1);var replacing=await Start();memory.Objects[replacing.Claim.StagingKey]=bytes;
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();(await db.CourseModules.FindAsync("video-module"))!.VideoStreamingUrl="https://example.invalid/otro.mp4";await db.SaveChangesAsync();}
  Check((await http.PostAsJsonAsync(path+"/complete",new{token=replacing.Token})).StatusCode==HttpStatusCode.Conflict,"Subida anterior no sobrescribe otro cambio de video");
  Check((await http.PostAsJsonAsync(path+"/cancel",new{token=replacing.Token})).IsSuccessStatusCode&&!memory.Objects.ContainsKey(replacing.Claim.StagingKey),"Cancelar limpia únicamente el temporal propio");
  memory.Fail=true;Check((await http.PostAsJsonAsync(path+"/upload",new{fileName="curso.mp4",size=64})).StatusCode==HttpStatusCode.ServiceUnavailable,"Fallo de RustFS produce respuesta recuperable");memory.Fail=false;
  Check((await http.DeleteAsync(path)).IsSuccessStatusCode&&memory.Objects.ContainsKey(upload.Claim.Key),"Quitar video lo desvincula conservando el objeto para recuperación");
  using(var scope=services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();Check((await db.CourseModules.FindAsync("video-module"))!.StreamingId==""&&await db.AuditLogs.AnyAsync(a=>a.Action=="PRIVATE_VIDEO_UPLOADED"),"Referencia y auditoría se guardan en la base");}
  Console.WriteLine($"{count} comprobaciones de video superadas. Solo memoria; no se cargaron archivos reales.");
 }
}
