using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicEvents;
using ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicEvent;
using ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicAdvisorys;
using ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicAdvisory;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed class PublicCatalogController(ISender sender) : ControllerBase
{
    [HttpGet("events")]
    public async Task<IActionResult> ListPublicEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListPublicEventsQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetPublicEvent(string id)
        => (await sender.Send(new GetPublicEventQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("advisories")]
    public async Task<IActionResult> ListPublicAdvisorys([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListPublicAdvisorysQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("advisories/{id}")]
    public async Task<IActionResult> GetPublicAdvisory(string id)
        => (await sender.Send(new GetPublicAdvisoryQuery(id),HttpContext.RequestAborted)).ToActionResult();
}
