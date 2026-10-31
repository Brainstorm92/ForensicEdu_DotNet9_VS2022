using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Settings;
namespace ForensicEdu.Application.Handlers.Settings.Queries.Social;

public sealed class SettingsSocialQueryHandler(IApplicationDbContext db) : IRequestHandler<SettingsSocialQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SettingsSocialQuery message, CancellationToken cancellationToken)
    {


        return ApplicationResult.Success(await db.SystemSocialSettings.Where(s => SettingsKeys.SocialKeys.Contains(s.Key))
        .Select(s => new { s.Key, s.Value }).ToListAsync(cancellationToken));
    }
}
