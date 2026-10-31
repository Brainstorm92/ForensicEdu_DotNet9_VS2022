using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Exams.Commands.Submit;
namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExamsController(ISender sender) : ControllerBase
{
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ExamSubmissionDto submission)
    {
        return (await sender.Send(new ExamsSubmitCommand(submission), HttpContext.RequestAborted)).ToActionResult();
    }
}
