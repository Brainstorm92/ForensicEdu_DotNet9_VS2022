using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListUsers;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetUser;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserStatus;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserRole;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListPayments;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPayment;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.ReviewPayment;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPaymentVoucher;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListSupportTickets;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSupportTicket;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetTicketStatus;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSummaryReport;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicEnrollments;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicAttempts;
using ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicCertificates;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public sealed class OperationsAdminController(ISender sender) : ControllerBase
{
    [HttpPost("users")]
    [Authorize(Roles="1")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
        => (await sender.Send(new ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.CreateUser.CreateUserCommand(input),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("users")]
    [Authorize(Roles="1,5")]
    public async Task<IActionResult> ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListUsersQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("users/{id}")]
    [Authorize(Roles="1,5")]
    public async Task<IActionResult> GetUser(string id)
        => (await sender.Send(new GetUserQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("users/{id}/status")]
    [Authorize(Roles="1")]
    public async Task<IActionResult> SetUserStatus(string id, [FromBody] UserStatusInput input)
        => (await sender.Send(new SetUserStatusCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("users/{id}/role")]
    [Authorize(Roles="1")]
    public async Task<IActionResult> SetUserRole(string id, [FromBody] UserRoleInput input)
        => (await sender.Send(new SetUserRoleCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("payments")]
    [Authorize(Roles="1,3")]
    public async Task<IActionResult> ListPayments([FromQuery] PaymentStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListPaymentsQuery(status, page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("payments/{id}")]
    [Authorize(Roles="1,3")]
    public async Task<IActionResult> GetPayment(string id)
        => (await sender.Send(new GetPaymentQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("payments/{id}/review")]
    [Authorize(Roles="1,3")]
    public async Task<IActionResult> ReviewPayment(string id, [FromBody] PaymentReviewInput input)
        => (await sender.Send(new ReviewPaymentCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("payments/{id}/voucher")]
    [Authorize(Roles="1,3")]
    public async Task<IActionResult> GetPaymentVoucher(string id)
        => (await sender.Send(new GetPaymentVoucherQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("tickets")]
    [Authorize(Roles="1,5")]
    public async Task<IActionResult> ListSupportTickets([FromQuery] TicketStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListSupportTicketsQuery(status, page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("tickets/{id}")]
    [Authorize(Roles="1,5")]
    public async Task<IActionResult> GetSupportTicket(string id)
        => (await sender.Send(new GetSupportTicketQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("tickets/{id}/status")]
    [Authorize(Roles="1,5")]
    public async Task<IActionResult> SetTicketStatus(string id, [FromBody] TicketStatusInput input)
        => (await sender.Send(new SetTicketStatusCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("reports/summary")]
    [Authorize(Roles="1")]
    public async Task<IActionResult> GetSummaryReport()
        => (await sender.Send(new GetSummaryReportQuery(),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("enrollments")]
    [Authorize(Roles="1,2")]
    public async Task<IActionResult> ListAcademicEnrollments([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListAcademicEnrollmentsQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("exam-attempts")]
    [Authorize(Roles="1,2")]
    public async Task<IActionResult> ListAcademicAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListAcademicAttemptsQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("certificates")]
    [Authorize(Roles="1,2")]
    public async Task<IActionResult> ListAcademicCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListAcademicCertificatesQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();
}
