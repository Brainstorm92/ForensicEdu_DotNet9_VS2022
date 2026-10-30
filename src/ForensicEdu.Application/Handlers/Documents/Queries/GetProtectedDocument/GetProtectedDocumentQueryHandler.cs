using System.Text;
using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Documents.Queries.GetProtectedDocument;
public sealed class GetProtectedDocumentQueryHandler(IApplicationDbContext db,ICurrentUser current,IDocumentStorage storage,IDocumentProcessor processor):IRequestHandler<GetProtectedDocumentQuery,ApplicationResult>
{
 public async Task<ApplicationResult> Handle(GetProtectedDocumentQuery request,CancellationToken ct)
 {
  var access=await PrivateDocuments.AuthorizeAsync(db,current,request.EnrollmentId,request.DocumentId,ct);
  if(access==null)return ApplicationResult.NotFound();
  var (document,user)=access.Value;var key=PrivateDocuments.Key(document);
  if(key==null)return ApplicationResult.Unavailable(new{message="Este material debe volver a subirse como archivo privado."});
  byte[]? bytes;
  try{bytes=await storage.ReadAsync(key,ct);}
  catch(OperationCanceledException) when(ct.IsCancellationRequested){throw;}
  catch{return ApplicationResult.Unavailable(new{message="El material no está disponible en este momento."});}
  if(bytes==null)return ApplicationResult.NotFound(new{message="No se encontró el archivo del material."});
  if(document.FileType=="DOCX"){try{bytes=Encoding.UTF8.GetBytes(processor.RenderWord(bytes,PrivateDocuments.Watermark(user)));}catch(InvalidDataException){return ApplicationResult.Invalid(new{message="No se pudo preparar la vista del documento Word."});}}
  EndpointSupport.Audit(db,current,"PRIVATE_DOCUMENT_VIEWED","CourseDocument",document.Id);await db.SaveChangesAsync(ct);
  return ApplicationResult.Download(bytes,document.FileType=="PDF"?"application/pdf":"text/html; charset=utf-8","");
 }
}
