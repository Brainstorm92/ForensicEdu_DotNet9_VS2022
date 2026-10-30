using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Settings;

namespace ForensicEdu.Application.Handlers.Settings.Commands.Set;
public sealed record SettingsSetCommand(string Key, SettingRequest Request) : IRequest<ApplicationResult>;
public sealed class SettingsSetCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SettingsSetCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SettingsSetCommand message, CancellationToken cancellationToken)
    {
        var key = message.Key;
        var request = message.Request;

        if (key == "ExamRetakeWaitHours")
        {
            if (!int.TryParse(request.Value, out var hours) || hours < 0 || hours > 168) return ApplicationResult.Invalid();
        }
        else if (!SettingsKeys.SocialKeys.Contains(key) || (!string.IsNullOrEmpty(request.Value) &&
            (!Uri.TryCreate(request.Value, UriKind.Absolute, out var uri) || uri.Scheme != "https")))
            return ApplicationResult.Invalid(new { message = "Use una red admitida y una URL HTTPS; vacío desactiva el enlace." });
        var setting = await db.SystemSocialSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) { setting = new SystemSocialSetting { Key = key }; db.SystemSocialSettings.Add(setting); }
        setting.Value = request.Value;
        setting.UpdatedAt = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog { UserId = current.UserId!, Action = "SETTING_CHANGED", EntityType = "SystemSocialSetting", EntityId = setting.Id, Details = key });
        await db.SaveChangesAsync(cancellationToken);
        return ApplicationResult.NoContent();
    }
}

public sealed class SettingsSetCommandValidator : AbstractValidator<SettingsSetCommand>
{
    public SettingsSetCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Value).NotNull().MaximumLength(2048);
        });
    }
}
