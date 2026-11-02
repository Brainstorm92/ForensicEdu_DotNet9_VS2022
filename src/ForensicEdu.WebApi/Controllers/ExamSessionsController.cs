using System.Text.Json;
using MediatR;using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Authorization;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;
using ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
using ForensicEdu.Application.Handlers.ExamSessions.Queries.Read;
namespace ForensicEdu.WebApi.Controllers;
[Authorize,ApiController,Route("api/exam-sessions")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class ExamSessionsController(ISender sender):ControllerBase
{
 [HttpPost("{enrollmentId}/start")]
 public async Task<IActionResult> Start(string enrollmentId)=>(await sender.Send(new ExamSessionsStartCommand(enrollmentId),HttpContext.RequestAborted)).ToActionResult();
 [HttpPut("{attemptId}/answers")]
 public async Task<IActionResult> Save(string attemptId,[FromBody]Dictionary<string,JsonElement> answers)=>(await sender.Send(new ExamSessionsSaveCommand(attemptId,answers,Request.Headers["If-Match"].FirstOrDefault()?.Trim('"')),HttpContext.RequestAborted)).ToActionResult();
 [HttpGet("active")]
 public async Task<IActionResult> Active()=>(await sender.Send(new ReadExamSessionQuery(),HttpContext.RequestAborted)).ToActionResult();
 [HttpGet("{attemptId}")]
 public async Task<IActionResult> Read(string attemptId)=>(await sender.Send(new ReadExamSessionQuery(attemptId),HttpContext.RequestAborted)).ToActionResult();
}
