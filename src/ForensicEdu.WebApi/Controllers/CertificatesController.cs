using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Certificates.Commands.Verify;
using ForensicEdu.Application.Handlers.Certificates.Queries.DownloadPdf;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController(ISender sender) : ControllerBase
{
    [HttpGet("verify/{code}")]
    public async Task<IActionResult> Verify(string code)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var userAgent = Request.Headers.UserAgent.ToString();
        return (await sender.Send(new CertificatesVerifyCommand(code, ip, userAgent), HttpContext.RequestAborted)).ToActionResult();
    }

    [Authorize]
    [HttpGet("download/{code}/pdf")]
    public async Task<IActionResult> DownloadPdf(string code)
    {
        return (await sender.Send(new CertificatesDownloadPdfQuery(code), HttpContext.RequestAborted)).ToActionResult();
    }
}
