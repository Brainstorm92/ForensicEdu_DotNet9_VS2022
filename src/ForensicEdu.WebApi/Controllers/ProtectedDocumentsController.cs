using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Handlers.Documents.Commands.UploadCourseDocument;
using ForensicEdu.Application.Handlers.Documents.Queries.GetProtectedDocument;
using ForensicEdu.WebApi.Common;
namespace ForensicEdu.WebApi.Controllers;
[ApiController]
[Authorize]
public sealed class ProtectedDocumentsController(ISender sender):ControllerBase
{
 [HttpPost("api/admin/courses/{courseId}/modules/{moduleId}/documents/upload")]
 [Authorize(Roles="1,2")]
 [RequestSizeLimit(22020096)]
 [RequestFormLimits(MultipartBodyLengthLimit=22020096)]
 public async Task<IActionResult> Upload(string courseId,string moduleId,[FromForm]string title,IFormFile file)
 {
  if(file==null || file.Length==0 || file.Length>PrivateDocuments.MaxBytes)return BadRequest(new{message="Selecciona un PDF o Word (.docx) de hasta 20 MB."});
  using var stream=new MemoryStream();await file.CopyToAsync(stream,HttpContext.RequestAborted);
  return (await sender.Send(new UploadCourseDocumentCommand(courseId,moduleId,title,file.FileName,stream.ToArray()),HttpContext.RequestAborted)).ToActionResult();
 }
 [HttpGet("api/student/enrollments/{enrollmentId}/documents/{documentId}/content")]
 public async Task<IActionResult> ReadContent(string enrollmentId,string documentId)
 {
  Response.Headers.CacheControl="no-store, private";Response.Headers.Pragma="no-cache";
  Response.Headers["X-Content-Type-Options"]="nosniff";Response.Headers["X-Frame-Options"]="DENY";
  Response.Headers.ContentSecurityPolicy="default-src 'none'; sandbox";
  var result=await sender.Send(new GetProtectedDocumentQuery(enrollmentId,documentId),HttpContext.RequestAborted);
  if(result.Outcome==ResultOutcome.Download && result.Value is DownloadContent content)return File(content.Bytes,content.ContentType);
  return result.ToActionResult();
 }
}
