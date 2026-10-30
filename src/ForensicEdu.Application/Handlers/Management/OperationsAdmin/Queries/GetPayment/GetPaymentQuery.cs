using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPayment;

public sealed record GetPaymentQuery(string Id) : IRequest<ApplicationResult>;
