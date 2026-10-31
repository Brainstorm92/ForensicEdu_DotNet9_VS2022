using FluentValidation;
namespace ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;

public sealed class AdminGetAuditLogsQueryValidator : AbstractValidator<AdminGetAuditLogsQuery>
{
    public AdminGetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }
}
