using ForensicEdu.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ForensicEdu.WebApi.Controllers;

[ApiController, Authorize, Route("api/student/notifications")]
public sealed class StudentNotificationsController(IStudentNotificationCenter notifications) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]bool unreadOnly=false)
    { Response.Headers.CacheControl="no-store";return Ok(await notifications.ListAsync(page,unreadOnly,HttpContext.RequestAborted)); }
    [HttpPut("{id}/read")] public async Task<IActionResult> Read(string id)
        => await notifications.ReadAsync(id,HttpContext.RequestAborted)?NoContent():NotFound();
    [HttpPut("read-all")] public async Task<IActionResult> ReadAll()
    { await notifications.ReadAllAsync(HttpContext.RequestAborted);return NoContent(); }
}
