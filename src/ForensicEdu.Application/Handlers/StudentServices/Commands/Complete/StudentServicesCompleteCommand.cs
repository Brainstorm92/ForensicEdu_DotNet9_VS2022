using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Complete;
public sealed record StudentServicesCompleteCommand(string Id, string ModuleId) : IRequest<ApplicationResult>;
public sealed class StudentServicesCompleteCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesCompleteCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesCompleteCommand message, CancellationToken cancellationToken)
    {
        var id = message.Id;
        var moduleId = message.ModuleId;

        var enrollment = await db.Enrollments.Include(e => e.Course!).ThenInclude(c => c.Modules)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == current.UserId && e.Status == "ACTIVA");
        if (enrollment?.Course == null) return ApplicationResult.NotFound();
        var modules = enrollment.Course.Modules.OrderBy(m => m.OrderIndex).ToList();
        var index = modules.FindIndex(m => m.Id == moduleId);
        if (index < 0) return ApplicationResult.NotFound();
        var completed = JsonSerializer.Deserialize<HashSet<string>>(enrollment.CompletedModulesJson) ?? new();
        if (modules.Take(index).Any(m => !completed.Contains(m.Id))) return ApplicationResult.Conflict(new { message = "Complete primero los módulos anteriores." });
        completed.Add(moduleId);
        enrollment.CompletedModulesJson = JsonSerializer.Serialize(completed);
        enrollment.ProgressPercent = modules.Count == 0 ? 0 : modules.Count(m => completed.Contains(m.Id)) * 100 / modules.Count;
        db.AuditLogs.Add(new AuditLog { UserId = current.UserId, Action = "MODULE_COMPLETED", EntityType = "Enrollment", EntityId = enrollment.Id, Details = moduleId });
        await db.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success(new { enrollment.ProgressPercent });
    }
}

public sealed class StudentServicesCompleteCommandValidator : AbstractValidator<StudentServicesCompleteCommand>
{
    public StudentServicesCompleteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ModuleId).NotEmpty().MaximumLength(200);
    }
}
