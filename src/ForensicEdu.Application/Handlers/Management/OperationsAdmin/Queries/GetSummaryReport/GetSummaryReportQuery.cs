using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSummaryReport;

public sealed record GetSummaryReportQuery() : IRequest<ApplicationResult>;
