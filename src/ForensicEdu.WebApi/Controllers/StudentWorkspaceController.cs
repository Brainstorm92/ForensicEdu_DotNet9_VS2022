using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyCertificates;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyPayments;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetClassroom;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetMyExamHistory;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetModuleVideo;
using ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/student")]
[Authorize]
public sealed class StudentWorkspaceController(ISender sender) : ControllerBase
{
    [HttpGet("certificates")]
    public async Task<IActionResult> ListMyCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListMyCertificatesQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("payments")]
    public async Task<IActionResult> ListMyPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListMyPaymentsQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("enrollments/{id}/classroom")]
    public async Task<IActionResult> GetClassroom(string id)
        => (await sender.Send(new GetClassroomQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("enrollments/{id}/attempts")]
    public async Task<IActionResult> GetMyExamHistory(string id)
        => (await sender.Send(new GetMyExamHistoryQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("enrollments/{id}/modules/{moduleId}/video")]
    public async Task<IActionResult> GetModuleVideo(string id, string moduleId)
        => (await sender.Send(new GetModuleVideoQuery(id, moduleId),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("enrollments/{id}/documents/{documentId}")]
    public async Task<IActionResult> GetDocumentAccess(string id, string documentId)
        => (await sender.Send(new GetDocumentAccessQuery(id, documentId),HttpContext.RequestAborted)).ToActionResult();
}
