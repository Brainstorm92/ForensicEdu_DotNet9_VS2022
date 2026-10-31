using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;
using ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/exam-sessions")]
public class ExamSessionsController(ISender sender) : ControllerBase
{
    [HttpPost("{enrollmentId}/start")]
    public async Task<IActionResult> Start(string enrollmentId)
    {
        return (await sender.Send(new ExamSessionsStartCommand(enrollmentId), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPut("{attemptId}/answers")]
    public async Task<IActionResult> Save(string attemptId, [FromBody] Dictionary<string, int> answers)
    {
        return (await sender.Send(new ExamSessionsSaveCommand(attemptId, answers), HttpContext.RequestAborted)).ToActionResult();
    }
}
