using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.ExamSessions.Commands.Save;
public sealed record ExamSessionsSaveCommand(string AttemptId, Dictionary<string, int> Answers) : IRequest<ApplicationResult>;
public sealed class ExamSessionsSaveCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ExamSessionsSaveCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ExamSessionsSaveCommand message, CancellationToken cancellationToken)
    {
        var attemptId = message.AttemptId;
        var answers = message.Answers;

        var userId = current.UserId;
        var attempt = await db.ExamAttempts.FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);
        if (attempt == null) return ApplicationResult.NotFound();
        if (attempt.CompletedAt != null || DateTime.UtcNow > attempt.StartedAt.AddMinutes(10))
            return ApplicationResult.Conflict(new { message = "La evaluación está cerrada." });
        using var snapshot = JsonDocument.Parse(attempt.QuestionsSnapshotJson);
        var ids = snapshot.RootElement.EnumerateArray().Select(q => q.GetProperty("id").GetString()).ToHashSet();
        if (answers.Any(a => !ids.Contains(a.Key) || a.Value < 0)) return ApplicationResult.Invalid();
        attempt.AnswersJson = JsonSerializer.Serialize(answers);
        await db.SaveChangesAsync(cancellationToken);
        return ApplicationResult.NoContent();
    }
}

public sealed class ExamSessionsSaveCommandValidator : AbstractValidator<ExamSessionsSaveCommand>
{
    public ExamSessionsSaveCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Answers).NotNull();
    }
}
