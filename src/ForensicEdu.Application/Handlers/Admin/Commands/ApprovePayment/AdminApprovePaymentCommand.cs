using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;

public sealed record AdminApprovePaymentCommand(string PaymentId, string? ReviewToken = null, string? ReviewReason = null) : IRequest<ApplicationResult>;
