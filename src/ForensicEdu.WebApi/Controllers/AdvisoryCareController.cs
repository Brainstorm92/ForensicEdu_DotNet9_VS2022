using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Contracts.AdvisoryCare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ForensicEdu.WebApi.Controllers;
[ApiController,Authorize,Route("api/advisory-care"),RequestSizeLimit(65536)]
public sealed class AdvisoryCareController(IAdvisoryCare care):ControllerBase
{
 private async Task<IActionResult> Run(Func<Task<object?>> operation){try{Response.Headers.CacheControl="no-store";var result=await operation();return result==null?NoContent():Ok(result);}catch(AdvisoryCareException e){return StatusCode(e.Failure switch{CareFailure.Invalid=>400,CareFailure.Forbidden=>403,CareFailure.NotFound=>404,_=>409},new{message=e.Message});}}
 [HttpGet]public Task<IActionResult> List(int page=1,int pageSize=20,string? search=null,string? status=null)=>Run(async()=>await care.ListAsync(page,pageSize,search,status,HttpContext.RequestAborted));
 [HttpGet("consultants"),Authorize(Roles="1,2")]public Task<IActionResult> Consultants()=>Run(async()=>await care.ConsultantsAsync(HttpContext.RequestAborted));
 [HttpGet("{id}")]public Task<IActionResult> Get(string id)=>Run(async()=>await care.GetAsync(id,HttpContext.RequestAborted));
 [HttpPut("{id}/assignment"),Authorize(Roles="1,2")]public Task<IActionResult> Assign(string id,CareAssignment input)=>Run(async()=>{await care.AssignAsync(id,input,HttpContext.RequestAborted);return null;});
 [HttpPut("{id}"),Authorize(Roles="1,2,7")]public Task<IActionResult> Update(string id,CareUpdate input)=>Run(async()=>{await care.UpdateAsync(id,input,HttpContext.RequestAborted);return null;});
 [HttpPost("{id}/sessions"),Authorize(Roles="1,2,7")]public Task<IActionResult> AddSession(string id,CareSessionInput input)=>Run(async()=>{await care.SaveSessionAsync(id,null,input,HttpContext.RequestAborted);return null;});
 [HttpPut("{id}/sessions/{sessionId}"),Authorize(Roles="1,2,7")]public Task<IActionResult> UpdateSession(string id,string sessionId,CareSessionInput input)=>Run(async()=>{await care.SaveSessionAsync(id,sessionId,input,HttpContext.RequestAborted);return null;});
}
