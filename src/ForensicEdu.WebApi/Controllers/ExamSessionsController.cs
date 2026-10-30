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
[Route("api/exam-sessions")]
public class ExamSessionsController(IApplicationDbContext db) : ControllerBase
{
    [HttpPost("{enrollmentId}/start")]
    public async Task<IActionResult> Start(string enrollmentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var enrollment = await db.Enrollments.Include(e => e.Course).Include(e => e.ExamAttempts)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId);
        if (enrollment == null) return NotFound();
        if (enrollment.Status != "ACTIVA" || enrollment.ProgressPercent < 100)
            return BadRequest(new { message = "Complete los materiales obligatorios de una matrícula activa." });
        if (!await db.Orders.AnyAsync(o => o.Id == enrollment.OrderId && o.UserId == userId && o.Status == OrderStatus.APROBADO))
            return BadRequest(new { message = "La matrícula debe tener una orden aprobada." });
        var previous = enrollment.ExamAttempts.Where(a => a.CompletedAt != null).OrderBy(a => a.AttemptNumber).ToList();
        if (previous.Any(a => a.Approved) || previous.Count >= 2) return Conflict(new { message = "No tiene intentos disponibles." });
        if (previous.LastOrDefault()?.NextAttemptAllowedAt > DateTime.UtcNow) return Conflict(new { message = "Debe esperar antes de la re-evaluación." });
        var attempt = enrollment.ExamAttempts.SingleOrDefault(a => a.CompletedAt == null);
        if (attempt == null)
        {
            using var doc = JsonDocument.Parse(enrollment.Course?.QuestionsJson ?? "[]");
            var questions = doc.RootElement.EnumerateArray().OrderBy(_ => System.Security.Cryptography.RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray();
            if (questions.Length == 0) return BadRequest(new { message = "El curso no tiene preguntas." });
            foreach (var q in questions)
                if (!q.TryGetProperty("id", out _) || !q.TryGetProperty("correctOptionIndex", out _))
                    return BadRequest(new { message = "Revise el formato del banco de preguntas." });
            attempt = new ExamAttempt { EnrollmentId = enrollment.Id, UserId = userId, CourseId = enrollment.CourseId,
                AttemptNumber = previous.Count + 1, StartedAt = DateTime.UtcNow, Status = ExamStatus.EN_CURSO,
                QuestionsSnapshotJson = JsonSerializer.Serialize(questions) };
            db.ExamAttempts.Add(attempt);
            enrollment.ExamStatus = ExamStatus.EN_CURSO;
            db.AuditLogs.Add(new AuditLog { UserId = userId, Action = "EXAM_STARTED", EntityType = "ExamAttempt", EntityId = attempt.Id });
            await db.SaveChangesAsync();
        }
        using var snapshot = JsonDocument.Parse(attempt.QuestionsSnapshotJson);
        var publicQuestions = snapshot.RootElement.EnumerateArray().Select(q => q.EnumerateObject()
            .Where(p => p.Name != "correctOptionIndex" && p.Name != "correctAnswerIndex" && p.Name != "explanation")
            .ToDictionary(p => p.Name, p => p.Value.Clone())).ToList();
        return Ok(new { attempt.Id, attempt.AttemptNumber, attempt.StartedAt, expiresAt = attempt.StartedAt.AddMinutes(10),
            serverTime = DateTime.UtcNow, questions = publicQuestions, savedAnswers = JsonSerializer.Deserialize<JsonElement>(attempt.AnswersJson) });
    }

    [HttpPut("{attemptId}/answers")]
    public async Task<IActionResult> Save(string attemptId, [FromBody] Dictionary<string, int> answers)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var attempt = await db.ExamAttempts.FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);
        if (attempt == null) return NotFound();
        if (attempt.CompletedAt != null || DateTime.UtcNow > attempt.StartedAt.AddMinutes(10))
            return Conflict(new { message = "La evaluación está cerrada." });
        using var snapshot = JsonDocument.Parse(attempt.QuestionsSnapshotJson);
        var ids = snapshot.RootElement.EnumerateArray().Select(q => q.GetProperty("id").GetString()).ToHashSet();
        if (answers.Any(a => !ids.Contains(a.Key) || a.Value < 0)) return BadRequest();
        attempt.AnswersJson = JsonSerializer.Serialize(answers);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
