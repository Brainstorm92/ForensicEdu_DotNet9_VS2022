using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;

public sealed class AdminGetAuditLogsQueryHandler(IApplicationDbContext _context) : IRequestHandler<AdminGetAuditLogsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AdminGetAuditLogsQuery message, CancellationToken cancellationToken)
    {
        var limit = message.Limit;

        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return ApplicationResult.Success(logs);
    }
}
