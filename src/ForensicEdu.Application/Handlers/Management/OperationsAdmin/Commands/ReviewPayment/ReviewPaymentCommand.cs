using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.ReviewPayment;

public sealed record ReviewPaymentCommand(string Id, PaymentReviewInput Input) : IRequest<ApplicationResult>;
