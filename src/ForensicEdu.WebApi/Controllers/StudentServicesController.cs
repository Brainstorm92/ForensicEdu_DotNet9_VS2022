using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Contracts.StudentServices;
using ForensicEdu.Application.Handlers.StudentServices.Queries.Enrollments;
using ForensicEdu.Application.Handlers.StudentServices.Commands.Complete;
using ForensicEdu.Application.Handlers.StudentServices.Queries.Events;
using ForensicEdu.Application.Handlers.StudentServices.Queries.EventAccess;
using ForensicEdu.Application.Handlers.StudentServices.Queries.Advisories;
using ForensicEdu.Application.Handlers.StudentServices.Queries.Tickets;
using ForensicEdu.Application.Handlers.StudentServices.Commands.CreateTicket;
using ForensicEdu.Application.Handlers.StudentServices.Commands.Reply;
namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/student")]
public class StudentServicesController(ISender sender) : ControllerBase
{
    [HttpGet("enrollments")]
    public async Task<IActionResult> Enrollments()
    {
        return (await sender.Send(new StudentServicesEnrollmentsQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPut("enrollments/{id}/modules/{moduleId}/complete")]
    public async Task<IActionResult> Complete(string id, string moduleId)
    {
        return (await sender.Send(new StudentServicesCompleteCommand(id, moduleId), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("events")]
    public async Task<IActionResult> Events()
    {
        return (await sender.Send(new StudentServicesEventsQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("events/{id}/access")]
    public async Task<IActionResult> EventAccess(string id)
    {
        return (await sender.Send(new StudentServicesEventAccessQuery(id), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("advisories")]
    public async Task<IActionResult> Advisories()
    {
        return (await sender.Send(new StudentServicesAdvisoriesQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets()
    {
        return (await sender.Send(new StudentServicesTicketsQuery(), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket(TicketRequest request)
    {
        return (await sender.Send(new StudentServicesCreateTicketCommand(request), HttpContext.RequestAborted)).ToActionResult();
    }

    [HttpPost("tickets/{id}/reply")]
    public async Task<IActionResult> Reply(string id, TicketReply request)
    {
        return (await sender.Send(new StudentServicesReplyCommand(id, request), HttpContext.RequestAborted)).ToActionResult();
    }
}
