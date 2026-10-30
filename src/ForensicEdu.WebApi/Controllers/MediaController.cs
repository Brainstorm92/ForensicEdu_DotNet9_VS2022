using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Handlers.Media.Commands.UploadImage;
using ForensicEdu.Application.Handlers.Media.Queries.GetImage;
using ForensicEdu.WebApi.Common;
namespace ForensicEdu.WebApi.Controllers;
[ApiController]
public sealed class MediaController(ISender sender):ControllerBase
{
 [Authorize(Roles="1,2")]
 [HttpPost("api/admin/media/images")]
 [RequestSizeLimit(5500000)]
 [RequestFormLimits(MultipartBodyLengthLimit=5500000)]
 public async Task<IActionResult> Upload(IFormFile file){
  if(file==null||file.Length==0||file.Length>5242880)return BadRequest(new{message="Selecciona una imagen JPG, PNG o WebP de hasta 5 MB."});
  using var stream=new MemoryStream();await file.CopyToAsync(stream,HttpContext.RequestAborted);
  return (await sender.Send(new UploadImageCommand(stream.ToArray()),HttpContext.RequestAborted)).ToActionResult();
 }
 [AllowAnonymous]
 [HttpGet("api/media/images/{id}")]
 public async Task<IActionResult> Get(string id){
  var result=await sender.Send(new GetImageQuery(id),HttpContext.RequestAborted);
  if(result.Outcome!=ResultOutcome.Download||result.Value is not DownloadContent image)return result.ToActionResult();
  Response.Headers.CacheControl="public,max-age=86400,immutable";Response.Headers["X-Content-Type-Options"]="nosniff";
  return File(image.Bytes,image.ContentType);
 }
}
