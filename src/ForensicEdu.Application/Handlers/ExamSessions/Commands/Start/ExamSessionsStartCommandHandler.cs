using MediatR;
using ForensicEdu.Application.Services;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;

public sealed class ExamSessionsStartCommandHandler(IApplicationDbContext db, ICurrentUser current, ICoursePurchaseGate gate) : IRequestHandler<ExamSessionsStartCommand, ApplicationResult>
{
    public Task<ApplicationResult> Handle(ExamSessionsStartCommand message, CancellationToken cancellationToken) => gate.ExecuteAsync(current.UserId!, "exam", () => Start(message,cancellationToken),cancellationToken);
    private async Task<ApplicationResult> Start(ExamSessionsStartCommand message,CancellationToken cancellationToken)
    {
        var account=await db.Users.AsNoTracking().FirstOrDefaultAsync(u=>u.Id==current.UserId,cancellationToken);
        if(account==null||account.ValidUntil<=DateTime.UtcNow||account.Status is AccountStatus.SUSPENDIDO or AccountStatus.VENCIDA or AccountStatus.PENDIENTE_DE_ELIMINACION or AccountStatus.ELIMINADO)return ApplicationResult.Forbidden(new{message="Tu cuenta no está habilitada para iniciar evaluaciones."});
        var enrollmentId = message.EnrollmentId;
        var other=await db.ExamAttempts.AnyAsync(a=>a.UserId==current.UserId&&a.CompletedAt==null&&a.EnrollmentId!=enrollmentId,cancellationToken);
        if(other)return ApplicationResult.Conflict(new{message="Primero finaliza la evaluación que ya tienes en curso."});

        var userId = current.UserId!;
        var enrollment = await db.Enrollments.Include(e => e.Course).Include(e => e.ExamAttempts)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.UserId == userId);
        if (enrollment == null) return ApplicationResult.NotFound();
        if (enrollment.Status != "ACTIVA" || enrollment.ProgressPercent < 100)
            return ApplicationResult.Invalid(new { message = "Complete los materiales obligatorios de una matrícula activa." });
        if (!await db.Orders.AnyAsync(o => o.Id == enrollment.OrderId && o.UserId == userId && o.Status == OrderStatus.APROBADO))
            return ApplicationResult.Invalid(new { message = "La matrícula debe tener una orden aprobada." });
        var previous = enrollment.ExamAttempts.Where(a => a.CompletedAt != null).OrderBy(a => a.AttemptNumber).ToList();
        if (previous.Any(a => a.Approved) || previous.Count >= 2) return ApplicationResult.Conflict(new { message = "No tiene intentos disponibles." });
        if (previous.LastOrDefault()?.NextAttemptAllowedAt > DateTime.UtcNow) return ApplicationResult.Conflict(new { message = "Debe esperar antes de la re-evaluación." });
        var attempt = enrollment.ExamAttempts.SingleOrDefault(a => a.CompletedAt == null);
        if (attempt == null)
        {
            using var doc = JsonDocument.Parse(enrollment.Course?.QuestionsJson ?? "[]");
            var questions = doc.RootElement.EnumerateArray().OrderBy(_ => System.Security.Cryptography.RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray();
            if (questions.Length == 0) return ApplicationResult.Invalid(new { message = "El curso no tiene preguntas." });
            foreach (var q in questions)
                if (!q.TryGetProperty("id", out _) || !q.TryGetProperty("correctOptionIndex", out _) || !q.TryGetProperty("options",out _) || !q.TryGetProperty("question",out _))
                    return ApplicationResult.Invalid(new { message = "Revise el formato del banco de preguntas." });
            attempt = new ExamAttempt { EnrollmentId = enrollment.Id, UserId = userId, CourseId = enrollment.CourseId,
                AttemptNumber = previous.Count + 1, StartedAt = DateTime.UtcNow, Status = ExamStatus.EN_CURSO,
                QuestionsSnapshotJson = JsonSerializer.Serialize(questions) };
            db.ExamAttempts.Add(attempt);
            enrollment.ExamStatus = ExamStatus.EN_CURSO;
            db.AuditLogs.Add(new AuditLog { UserId = userId, Action = "EXAM_STARTED", EntityType = "ExamAttempt", EntityId = attempt.Id });
            await db.SaveChangesAsync(cancellationToken);
        }
        return ApplicationResult.Success(ExamAnswers.Session(attempt));
    }
}
