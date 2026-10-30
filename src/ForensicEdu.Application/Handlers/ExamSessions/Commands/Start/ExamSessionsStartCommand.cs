using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Start;
public sealed record ExamSessionsStartCommand(string EnrollmentId) : IRequest<ApplicationResult>;
public sealed class ExamSessionsStartCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ExamSessionsStartCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ExamSessionsStartCommand message, CancellationToken cancellationToken)
    {
        var enrollmentId = message.EnrollmentId;

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
                if (!q.TryGetProperty("id", out _) || !q.TryGetProperty("correctOptionIndex", out _))
                    return ApplicationResult.Invalid(new { message = "Revise el formato del banco de preguntas." });
            attempt = new ExamAttempt { EnrollmentId = enrollment.Id, UserId = userId, CourseId = enrollment.CourseId,
                AttemptNumber = previous.Count + 1, StartedAt = DateTime.UtcNow, Status = ExamStatus.EN_CURSO,
                QuestionsSnapshotJson = JsonSerializer.Serialize(questions) };
            db.ExamAttempts.Add(attempt);
            enrollment.ExamStatus = ExamStatus.EN_CURSO;
            db.AuditLogs.Add(new AuditLog { UserId = userId, Action = "EXAM_STARTED", EntityType = "ExamAttempt", EntityId = attempt.Id });
            await db.SaveChangesAsync(cancellationToken);
        }
        using var snapshot = JsonDocument.Parse(attempt.QuestionsSnapshotJson);
        var publicQuestions = snapshot.RootElement.EnumerateArray().Select(q => q.EnumerateObject()
            .Where(p => p.Name != "correctOptionIndex" && p.Name != "correctAnswerIndex" && p.Name != "explanation")
            .ToDictionary(p => p.Name, p => p.Value.Clone())).ToList();
        return ApplicationResult.Success(new { attempt.Id, attempt.AttemptNumber, attempt.StartedAt, expiresAt = attempt.StartedAt.AddMinutes(10),
            serverTime = DateTime.UtcNow, questions = publicQuestions, savedAnswers = JsonSerializer.Deserialize<JsonElement>(attempt.AnswersJson) });
    }
}

public sealed class ExamSessionsStartCommandValidator : AbstractValidator<ExamSessionsStartCommand>
{
    public ExamSessionsStartCommandValidator()
    {
        RuleFor(x => x.EnrollmentId).NotEmpty().MaximumLength(200);
    }
}
