using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Admin;

namespace ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;
public sealed record AdminGetAuditLogsQuery(int Limit = 50) : IRequest<ApplicationResult>;
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

public sealed class AdminGetAuditLogsQueryValidator : AbstractValidator<AdminGetAuditLogsQuery>
{
    public AdminGetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }
}
