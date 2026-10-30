using System.Security.Claims;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/student")]
public class StudentServicesController(IApplicationDbContext db) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("enrollments")]
    public async Task<IActionResult> Enrollments() => Ok(await db.Enrollments.Where(e => e.UserId == UserId)
        .Select(e => new { e.Id, e.CourseId, e.Status, e.ProgressPercent, e.ExamStatus, e.FinalGrade, e.AttemptsUsed, e.SecondAttemptAvailableAt }).ToListAsync());

    [HttpPut("enrollments/{id}/modules/{moduleId}/complete")]
    public async Task<IActionResult> Complete(string id, string moduleId)
    {
        var enrollment = await db.Enrollments.Include(e => e.Course!).ThenInclude(c => c.Modules)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == UserId && e.Status == "ACTIVA");
        if (enrollment?.Course == null) return NotFound();
        var modules = enrollment.Course.Modules.OrderBy(m => m.OrderIndex).ToList();
        var index = modules.FindIndex(m => m.Id == moduleId);
        if (index < 0) return NotFound();
        var completed = JsonSerializer.Deserialize<HashSet<string>>(enrollment.CompletedModulesJson) ?? new();
        if (modules.Take(index).Any(m => !completed.Contains(m.Id))) return Conflict(new { message = "Complete primero los módulos anteriores." });
        completed.Add(moduleId);
        enrollment.CompletedModulesJson = JsonSerializer.Serialize(completed);
        enrollment.ProgressPercent = modules.Count == 0 ? 0 : modules.Count(m => completed.Contains(m.Id)) * 100 / modules.Count;
        db.AuditLogs.Add(new AuditLog { UserId = UserId, Action = "MODULE_COMPLETED", EntityType = "Enrollment", EntityId = enrollment.Id, Details = moduleId });
        await db.SaveChangesAsync();
        return Ok(new { enrollment.ProgressPercent });
    }

    [HttpGet("events")]
    public async Task<IActionResult> Events()
    {
        var products = db.Orders.Where(o => o.UserId == UserId && o.Status == OrderStatus.APROBADO && o.ProductType == ProductType.EVENTO).Select(o => o.ProductId);
        return Ok(await db.LiveEvents.Where(e => products.Contains(e.Id))
            .Select(e => new { e.Id, e.Title, e.EventDate, e.TimeSlot, e.AccessStatus }).ToListAsync());
    }

    [HttpGet("events/{id}/access")]
    public async Task<IActionResult> EventAccess(string id)
    {
        if (!await db.Orders.AnyAsync(o => o.UserId == UserId && o.ProductId == id && o.ProductType == ProductType.EVENTO && o.Status == OrderStatus.APROBADO)) return Forbid();
        var item = await db.LiveEvents.FirstOrDefaultAsync(e => e.Id == id);
        if (item == null) return NotFound();
        if (item.AccessStatus == "CANCELADO" || item.AccessStatus == "FINALIZADO" || DateTime.UtcNow < item.EventDate.AddMinutes(-15))
            return Conflict(new { message = "La sala todavía no está habilitada o el evento ha finalizado." });
        if (string.IsNullOrWhiteSpace(item.RoomUrl)) return StatusCode(503, new { message = "La sala está pendiente de configuración." });
        return Ok(new { url = item.RoomUrl });
    }

    [HttpGet("advisories")]
    public async Task<IActionResult> Advisories()
    {
        var products = db.Orders.Where(o => o.UserId == UserId && o.Status == OrderStatus.APROBADO && o.ProductType == ProductType.ASESORIA).Select(o => o.ProductId);
        return Ok(await db.AdvisoryServices.Where(a => products.Contains(a.Id))
            .Select(a => new { a.Id, a.Title, a.Specialist, a.Specialty, a.WhatsappContact }).ToListAsync());
    }

    public record TicketRequest(string Subject, string Category, string Message);
    public record TicketReply(string Message);

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets() => Ok(await db.SupportTickets.Where(t => t.UserId == UserId)
        .OrderByDescending(t => t.CreatedAt).ToListAsync());

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket(TicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 10000) return BadRequest();
        var user = await db.Users.FirstAsync(u => u.Id == UserId);
        var ticket = new SupportTicket { UserId = UserId, UserName = user.FullName, UserEmail = user.Email,
            Subject = request.Subject.Trim(), Category = request.Category, Message = request.Message.Trim(),
            TicketCode = $"TK-{DateTime.UtcNow.Year}-{Guid.NewGuid():N}" };
        db.SupportTickets.Add(ticket);
        db.AuditLogs.Add(new AuditLog { UserId = UserId, Action = "TICKET_CREATED", EntityType = "SupportTicket", EntityId = ticket.Id });
        await db.SaveChangesAsync();
        return Ok(ticket);
    }

    [HttpPost("tickets/{id}/reply")]
    public async Task<IActionResult> Reply(string id, TicketReply request)
    {
        var ticket = await db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound();
        var staff = User.IsInRole("1") || User.IsInRole("5");
        if (ticket.UserId != UserId && !staff) return Forbid();
        if (ticket.Status == TicketStatus.CERRADO || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 10000) return BadRequest();
        var responses = JsonSerializer.Deserialize<List<JsonElement>>(ticket.ResponsesJson) ?? new();
        responses.Add(JsonSerializer.SerializeToElement(new { authorId = UserId, message = request.Message.Trim(), timestamp = DateTime.UtcNow }));
        ticket.ResponsesJson = JsonSerializer.Serialize(responses);
        ticket.Status = staff ? TicketStatus.RESPONDIDO : TicketStatus.ABIERTO;
        db.AuditLogs.Add(new AuditLog { UserId = UserId, Action = "TICKET_REPLIED", EntityType = "SupportTicket", EntityId = ticket.Id });
        await db.SaveChangesAsync();
        return Ok(ticket);
    }
}
