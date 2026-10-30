using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Admin.Queries.GetAuditLogs;

public sealed record AdminGetAuditLogsQuery(int Limit = 50) : IRequest<ApplicationResult>;
