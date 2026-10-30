using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Documents.Commands.UploadCourseDocument;
public sealed class UploadCourseDocumentCommandHandler(IApplicationDbContext db,ICurrentUser current,IDocumentStorage storage,IDocumentProcessor processor):IRequestHandler<UploadCourseDocumentCommand,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(UploadCourseDocumentCommand request,CancellationToken ct)
 {
  if(!EndpointSupport.HasRole(current,"1","2"))return ApplicationResult.Forbidden();
  var module=await db.CourseModules.FirstOrDefaultAsync(x=>x.Id==request.ModuleId && x.CourseId==request.CourseId,ct);
  if(module==null)return ApplicationResult.NotFound();
  string type;
  try{type=processor.Validate(request.Bytes,request.FileName);}catch(InvalidDataException e){return ApplicationResult.Invalid(new{message=e.Message});}
  var item=new CourseDocument{ModuleId=module.Id,Title=request.Title.Trim(),FileType=type,FileSize=$"{request.Bytes.Length/1048576d:0.00} MB",RequireWatermark=true};
  var key=$"documents/{Guid.Parse(item.Id):N}.{type.ToLowerInvariant()}";item.StorageUrl="rustfs://"+key;
  try{await storage.SaveAsync(key,request.Bytes,type=="PDF"?"application/pdf":"application/vnd.openxmlformats-officedocument.wordprocessingml.document",ct);}
  catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}
  catch{return ApplicationResult.Unavailable(new{message="No se pudo guardar el material. Inténtalo nuevamente."});}
  db.CourseDocuments.Add(item);EndpointSupport.Audit(db,current,"PRIVATE_DOCUMENT_UPLOADED","CourseDocument",item.Id);
  try{await db.SaveChangesAsync(ct);}catch{try{await storage.DeleteAsync(key,CancellationToken.None);}catch{}throw;}
  return ApplicationResult.Success(new{item.Id,item.Title,item.FileType,item.FileSize,item.RequireWatermark});
 }
}
