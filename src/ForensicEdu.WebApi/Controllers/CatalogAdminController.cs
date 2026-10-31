using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ForensicEdu.Domain.Enums;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.WebApi.Common;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedCourses;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedCourse;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateCourse;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateCourse;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.ArchiveCourse;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateModule;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateModule;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteModule;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateDocument;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateDocument;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteDocument;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetQuestionBank;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.SaveQuestionBank;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedEvents;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedEvent;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateEvent;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateEvent;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CancelEvent;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedAdvisorys;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedAdvisory;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateAdvisory;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateAdvisory;
using ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteAdvisory;
namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles="1,2") ]
public sealed class CatalogAdminController(ISender sender) : ControllerBase
{
    [HttpGet("courses")]
    public async Task<IActionResult> ListManagedCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListManagedCoursesQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("courses/{id}")]
    public async Task<IActionResult> GetManagedCourse(string id)
        => (await sender.Send(new GetManagedCourseQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CourseInput input)
        => (await sender.Send(new CreateCourseCommand(input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("courses/{id}")]
    public async Task<IActionResult> UpdateCourse(string id, [FromBody] CourseInput input)
        => (await sender.Send(new UpdateCourseCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> ArchiveCourse(string id)
        => (await sender.Send(new ArchiveCourseCommand(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPost("courses/{id}/modules")]
    public async Task<IActionResult> CreateModule(string id, [FromBody] ModuleInput input)
        => (await sender.Send(new CreateModuleCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("courses/{id}/modules/{moduleId}")]
    public async Task<IActionResult> UpdateModule(string id, string moduleId, [FromBody] ModuleInput input)
        => (await sender.Send(new UpdateModuleCommand(id, moduleId, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpDelete("courses/{id}/modules/{moduleId}")]
    public async Task<IActionResult> DeleteModule(string id, string moduleId)
        => (await sender.Send(new DeleteModuleCommand(id, moduleId),HttpContext.RequestAborted)).ToActionResult();

    [HttpPost("courses/{id}/modules/{moduleId}/documents")]
    public async Task<IActionResult> CreateDocument(string id, string moduleId, [FromBody] DocumentInput input)
        => (await sender.Send(new CreateDocumentCommand(id, moduleId, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("courses/{id}/modules/{moduleId}/documents/{documentId}")]
    public async Task<IActionResult> UpdateDocument(string id, string moduleId, string documentId, [FromBody] DocumentInput input)
        => (await sender.Send(new UpdateDocumentCommand(id, moduleId, documentId, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpDelete("courses/{id}/modules/{moduleId}/documents/{documentId}")]
    public async Task<IActionResult> DeleteDocument(string id, string moduleId, string documentId)
        => (await sender.Send(new DeleteDocumentCommand(id, moduleId, documentId),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("courses/{id}/questions")]
    public async Task<IActionResult> GetQuestionBank(string id)
        => (await sender.Send(new GetQuestionBankQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("courses/{id}/questions")]
    public async Task<IActionResult> SaveQuestionBank(string id, [FromBody] QuestionBankInput input)
        => (await sender.Send(new SaveQuestionBankCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("events")]
    public async Task<IActionResult> ListManagedEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListManagedEventsQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("events/{id}")]
    public async Task<IActionResult> GetManagedEvent(string id)
        => (await sender.Send(new GetManagedEventQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] EventInput input)
        => (await sender.Send(new CreateEventCommand(input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("events/{id}")]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] EventInput input)
        => (await sender.Send(new UpdateEventCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpDelete("events/{id}")]
    public async Task<IActionResult> CancelEvent(string id)
        => (await sender.Send(new CancelEventCommand(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("advisories")]
    public async Task<IActionResult> ListManagedAdvisorys([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await sender.Send(new ListManagedAdvisorysQuery(page, pageSize, search),HttpContext.RequestAborted)).ToActionResult();

    [HttpGet("advisories/{id}")]
    public async Task<IActionResult> GetManagedAdvisory(string id)
        => (await sender.Send(new GetManagedAdvisoryQuery(id),HttpContext.RequestAborted)).ToActionResult();

    [HttpPost("advisories")]
    public async Task<IActionResult> CreateAdvisory([FromBody] AdvisoryInput input)
        => (await sender.Send(new CreateAdvisoryCommand(input),HttpContext.RequestAborted)).ToActionResult();

    [HttpPut("advisories/{id}")]
    public async Task<IActionResult> UpdateAdvisory(string id, [FromBody] AdvisoryInput input)
        => (await sender.Send(new UpdateAdvisoryCommand(id, input),HttpContext.RequestAborted)).ToActionResult();

    [HttpDelete("advisories/{id}")]
    public async Task<IActionResult> DeleteAdvisory(string id)
        => (await sender.Send(new DeleteAdvisoryCommand(id),HttpContext.RequestAborted)).ToActionResult();
}
