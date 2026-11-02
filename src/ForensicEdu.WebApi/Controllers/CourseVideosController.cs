using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.WebApi.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ForensicEdu.WebApi.Controllers;
[ApiController,Authorize(Roles="1,2"),Route("api/admin/courses/{courseId}/modules/{moduleId}/video"),RequestSizeLimit(16384)]
public sealed class CourseVideosController(IVideoAdministration videos):ControllerBase
{
 [HttpPost("upload")]public async Task<IActionResult> Begin(string courseId,string moduleId,VideoUploadInput input){Response.Headers.CacheControl="no-store";return(await videos.BeginAsync(courseId,moduleId,input,HttpContext.RequestAborted)).ToActionResult();}
 [HttpPost("complete")]public async Task<IActionResult> Finish(string courseId,string moduleId,VideoUploadFinish input)=>(await videos.FinishAsync(courseId,moduleId,input.Token,HttpContext.RequestAborted)).ToActionResult();
 [HttpPost("cancel")]public async Task<IActionResult> Cancel(string courseId,string moduleId,VideoUploadFinish input)=>(await videos.CancelAsync(courseId,moduleId,input.Token,HttpContext.RequestAborted)).ToActionResult();
 [HttpGet]public async Task<IActionResult> Preview(string courseId,string moduleId){Response.Headers.CacheControl="no-store";return(await videos.PreviewAsync(courseId,moduleId,HttpContext.RequestAborted)).ToActionResult();}
 [HttpDelete]public async Task<IActionResult> Remove(string courseId,string moduleId)=>(await videos.RemoveAsync(courseId,moduleId,HttpContext.RequestAborted)).ToActionResult();
}
