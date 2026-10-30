using ForensicEdu.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
namespace ForensicEdu.WebApi.Common;

public static class ApplicationResultExtensions
{
    public static IActionResult ToActionResult(this ApplicationResult result) => result.Outcome switch
    {
        ResultOutcome.Success => new OkObjectResult(result.Value),
        ResultOutcome.NoContent => new NoContentResult(),
        ResultOutcome.Invalid => new BadRequestObjectResult(result.Value),
        ResultOutcome.NotFound => new NotFoundObjectResult(result.Value),
        ResultOutcome.Unauthorized => new UnauthorizedObjectResult(result.Value),
        ResultOutcome.Forbidden => new ObjectResult(result.Value) { StatusCode = 403 },
        ResultOutcome.Conflict => new ConflictObjectResult(result.Value),
        ResultOutcome.Unavailable => new ObjectResult(result.Value) { StatusCode = 503 },
        ResultOutcome.Download when result.Value is DownloadContent file => new FileContentResult(file.Bytes, file.ContentType) { FileDownloadName = file.FileName },
        _ => throw new InvalidOperationException("Resultado de aplicación desconocido.")
    };
}
