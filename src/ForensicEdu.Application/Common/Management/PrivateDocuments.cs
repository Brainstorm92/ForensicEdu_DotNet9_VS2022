using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Common.Management;

public static class PrivateDocuments
{
 public const int MaxBytes = 20 * 1024 * 1024;
 public static string? Key(CourseDocument document)
 {
  var extension=document.FileType=="PDF"?"pdf":document.FileType=="DOCX"?"docx":null;
  if(extension==null || !Guid.TryParse(document.Id,out var id))return null;
  var key=$"documents/{id:N}.{extension}";
  return document.StorageUrl=="rustfs://"+key?key:null;
 }
 public static async Task<(CourseDocument Document,User User)?> AuthorizeAsync(IApplicationDbContext db, ICurrentUser current, string enrollmentId,string documentId,CancellationToken ct)
 {
  var user=await db.Users.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==current.UserId,ct);
  if(user==null || user.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION or AccountStatus.VENCIDA || user.ValidUntil<=DateTime.UtcNow)return null;
  var enrollment=await db.Enrollments.AsNoTracking().Include(x=>x.Course).ThenInclude(x=>x!.Modules).ThenInclude(x=>x.Documents)
   .FirstOrDefaultAsync(x=>x.Id==enrollmentId && x.UserId==current.UserId,ct);
  if(enrollment?.Course==null || enrollment.Status is not ("ACTIVA" or "COMPLETADA"))return null;
  if(!await db.Orders.AnyAsync(x=>x.Id==enrollment.OrderId && x.UserId==current.UserId && x.Status==OrderStatus.APROBADO && x.ProductType==ProductType.CURSO && x.ProductId==enrollment.CourseId,ct))return null;
  var doc=enrollment.Course.Modules.SelectMany(x=>x.Documents).FirstOrDefault(x=>x.Id==documentId);
  return doc==null?null:(doc,user);
 }
 public static string Watermark(User user)=>$"{(string.IsNullOrWhiteSpace(user.FullName)?user.Nombres+" "+user.Apellidos:user.FullName)} · {user.NumeroDocumento}{(string.IsNullOrWhiteSpace(user.NumeroDocumento)?user.Dni:"")} · {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC · Uso personal";
}
