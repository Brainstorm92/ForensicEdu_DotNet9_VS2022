using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Orders;
namespace ForensicEdu.Application.Handlers.Orders.Commands.CreateOrderAndSubmitPayment;

public sealed record OrdersCreateOrderAndSubmitPaymentCommand(CreateOrderRequest Request) : IRequest<ApplicationResult>;
