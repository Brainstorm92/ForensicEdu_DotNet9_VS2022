using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;

public class EvaluationService : IEvaluationService
{
    private readonly IApplicationDbContext _context;

    public EvaluationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExamResultDto> SubmitExamAttemptAsync(ExamSubmissionDto submission, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.User)
            .Include(e => e.ExamAttempts)
            .Include(e => e.DigitalCertificate)
            .FirstOrDefaultAsync(e => e.Id == submission.EnrollmentId && e.UserId == submission.UserId, cancellationToken);

        if (enrollment == null)
        {
            throw new InvalidOperationException("Matrícula no encontrada para el estudiante especificado.");
        }

        var course = enrollment.Course;
        if (course == null)
        {
            throw new InvalidOperationException("El curso asociado a esta matrícula no existe.");
        }

        var existingAttempts = enrollment.ExamAttempts.Where(a => a.CompletedAt != null).OrderBy(a => a.AttemptNumber).ToList();
        var attemptCount = existingAttempts.Count;

        if (attemptCount >= 2)
        {
            throw new InvalidOperationException("Ha agotado el número máximo de intentos permitidos (2 intentos).");
        }

        if (attemptCount == 1)
        {
            var firstAttempt = existingAttempts[0];
            if (firstAttempt.Approved)
            {
                throw new InvalidOperationException("Usted ya aprobó este curso en el 1º intento (Nota: " + firstAttempt.Score + "/20). No requiere ni está habilitado para un 2º intento.");
            }

            // Check 24-hour rule
            if (firstAttempt.NextAttemptAllowedAt.HasValue && DateTime.UtcNow < firstAttempt.NextAttemptAllowedAt.Value)
            {
                var remaining = firstAttempt.NextAttemptAllowedAt.Value - DateTime.UtcNow;
                throw new InvalidOperationException($"Debe esperar 24 horas pedagógicas de repaso antes del 2º intento. Tiempo restante: {remaining.Hours}h {remaining.Minutes}m.");
            }
        }

        var attempt = enrollment.ExamAttempts.SingleOrDefault(a => a.CompletedAt == null)
            ?? throw new InvalidOperationException("Primero debe iniciar el examen en el servidor.");
        var answers = submission.SelectedAnswers;
        if (DateTime.UtcNow > attempt.StartedAt.AddSeconds(600))
            answers = JsonSerializer.Deserialize<Dictionary<string, int>>(attempt.AnswersJson) ?? new();
        int totalQuestions;
        int correctCount = 0;
        try
        {
            using var doc = JsonDocument.Parse(attempt.QuestionsSnapshotJson);
            var questions = doc.RootElement.EnumerateArray().ToList();
            totalQuestions = questions.Count;
            if (totalQuestions == 0) throw new InvalidOperationException("No hay preguntas configuradas.");
            foreach (var q in questions)
            {
                var id = q.GetProperty("id").GetString()!;
                if (answers.TryGetValue(id, out var selected) && selected == q.GetProperty("correctOptionIndex").GetInt32())
                    correctCount++;
            }
        }
        catch (JsonException ex) { throw new InvalidOperationException("Banco de preguntas inválido.", ex); }

        // Scale 0 to 20
        decimal score = totalQuestions > 0 ? Math.Round(((decimal)correctCount / totalQuestions) * 20m, 2) : 0m;
        bool approved = score >= 13.0m;
        var currentAttemptNumber = attemptCount + 1;

        DateTime? nextAttemptAllowedAt = null;
        ExamStatus attemptStatus;

        if (approved)
        {
            attemptStatus = ExamStatus.APROBADO;
        }
        else
        {
            if (currentAttemptNumber == 1)
            {
                attemptStatus = ExamStatus.DESAPROBADO_TEMPORAL;
                var configuredWait = await _context.SystemSocialSettings.Where(s => s.Key == "ExamRetakeWaitHours")
                    .Select(s => s.Value).FirstOrDefaultAsync(cancellationToken);
                var waitHours = int.TryParse(configuredWait, out var configuredHours) && configuredHours >= 0 && configuredHours <= 168 ? configuredHours : 24;
                nextAttemptAllowedAt = DateTime.UtcNow.AddHours(waitHours);
            }
            else
            {
                attemptStatus = ExamStatus.DESAPROBADO_DEFINITIVO;
            }
        }

        attempt.Score = score;
        attempt.Approved = approved;
        attempt.Status = attemptStatus;
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.DurationSeconds = (int)Math.Min(600, (DateTime.UtcNow - attempt.StartedAt).TotalSeconds);
        attempt.AnswersJson = JsonSerializer.Serialize(answers);
        attempt.NextAttemptAllowedAt = nextAttemptAllowedAt;
        enrollment.AttemptsUsed = currentAttemptNumber;
        enrollment.SecondAttemptAvailableAt = nextAttemptAllowedAt;
        if (currentAttemptNumber == 1) enrollment.FirstAttemptScore = score;
        else enrollment.SecondAttemptScore = score;

        // Update Enrollment Status
        string? issuedCertCode = null;
        if (approved)
        {
            enrollment.ExamStatus = ExamStatus.APROBADO;
            enrollment.FinalGrade = score;
            enrollment.CompletedAt = DateTime.UtcNow;

            // Generate Digital Certificate (Section 13)
            var year = DateTime.UtcNow.Year;
            var shortCode = course.ShortCode?.ToUpperInvariant() ?? "FOR";
            var certCount = Guid.NewGuid().ToString("N").ToUpperInvariant();
            issuedCertCode = $"CF-{year}-{shortCode}-{certCount}";

            var certificate = new DigitalCertificate
            {
                EnrollmentId = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = course.Id,
                CertificateCode = issuedCertCode,
                QrValidationUrl = $"https://forensic-edu.org/verificar/{issuedCertCode}",
                IssueDate = DateTime.UtcNow,
                StudentFullName = enrollment.User?.FullName ?? "Alumno",
                StudentDni = enrollment.User?.Dni ?? "00000000",
                CourseTitle = course.Title,
                AcademicHours = course.AcademicHours,
                FinalGrade = score,
                Status = CertificateStatus.VALIDO
            };

            _context.DigitalCertificates.Add(certificate);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = enrollment.UserId,
                Action = "CERTIFICATE_ISSUED",
                EntityType = "DigitalCertificate",
                EntityId = certificate.Id,
                Details = $"Certificado emitido con éxito: Código {issuedCertCode}, Nota {score}/20 (Intento {currentAttemptNumber}).",
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            enrollment.ExamStatus = attemptStatus;
            enrollment.FinalGrade = score;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = enrollment.UserId,
                Action = currentAttemptNumber == 1 ? "EXAM_FAILED_FIRST_ATTEMPT" : "EXAM_FAILED_DEFINITIVE",
                EntityType = "ExamAttempt",
                EntityId = attempt.Id,
                Details = $"Examen rendido con nota {score}/20 ({attemptStatus}).",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        string message = approved
            ? $"¡Felicitaciones! Has aprobado satisfactoriamente con nota {score}/20 en el Intento {currentAttemptNumber}. Tu certificado oficial {issuedCertCode} ha sido emitido con validación QR pública."
            : (currentAttemptNumber == 1
                ? $"Has obtenido {score}/20 (Nota mínima aprobatoria: 13/20). Cuentas con un 2º y último intento disponible tras 24 horas pedagógicas de repaso."
                : $"Has obtenido {score}/20 en el 2º intento. Has quedado en condición de DESAPROBADO DEFINITIVO sin derecho a emisión de certificado.");

        return new ExamResultDto(
            AttemptId: attempt.Id,
            AttemptNumber: currentAttemptNumber,
            Score: score,
            Approved: approved,
            Status: attemptStatus.ToString(),
            CorrectCount: correctCount,
            TotalQuestions: totalQuestions,
            NextAttemptAllowedAt: nextAttemptAllowedAt,
            CertificateCode: issuedCertCode,
            Message: message
        );
    }
}
