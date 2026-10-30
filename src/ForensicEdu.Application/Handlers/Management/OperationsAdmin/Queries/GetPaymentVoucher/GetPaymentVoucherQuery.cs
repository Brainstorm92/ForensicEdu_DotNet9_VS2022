using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPaymentVoucher;

public sealed record GetPaymentVoucherQuery(string Id) : IRequest<ApplicationResult>;
