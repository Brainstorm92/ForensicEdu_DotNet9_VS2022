using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Contracts.Admin;
using ForensicEdu.Application.Handlers.Admin.Queries.GetPendingPayments;
using ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;
using ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;
using ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;
namespace ForensicEdu.WebApi.Controllers;

[Authorize(Roles = "1,2,3")]
[ApiController]
[Route("api/[controller]")]
public class AdminController(ISender sender) : ControllerBase
{
    [HttpGet("pending-payments")]
    public async Task<IActionResult> GetPendingPayments()
    {
        return (await sender.Send(new AdminGetPendingPaymentsQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [Authorize(Roles = "1,3")]
    [HttpPost("payments/{paymentId}/approve")]
    public async Task<IActionResult> ApprovePayment(string paymentId, [FromQuery] string? adminUserId = null)
    {
        return (await sender.Send(new AdminApprovePaymentCommand(paymentId), HttpContext.RequestAborted)).ToActionResult();
    }

    [Authorize(Roles = "1,2")]
    [HttpPost("certificates/{code}/revoke")]
    public async Task<IActionResult> RevokeCertificate(string code, [FromBody] RevokeRequest req, [FromQuery] string? adminUserId = null)
    {
        return (await sender.Send(new AdminRevokeCertificateCommand(code, req), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 50)
    {
        return (await sender.Send(new AdminGetAuditLogsQuery(limit), HttpContext.RequestAborted)).ToActionResult();
    }
}
