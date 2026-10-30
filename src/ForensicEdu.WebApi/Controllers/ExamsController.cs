using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;

    public ExamsController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ExamSubmissionDto submission)
    {
        if (submission.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        try
        {
            var result = await _evaluationService.SubmitExamAttemptAsync(submission);
            return Ok(result);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
