using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Admin.Queries.GetPendingPayments;

public sealed record AdminGetPendingPaymentsQuery() : IRequest<ApplicationResult>;
