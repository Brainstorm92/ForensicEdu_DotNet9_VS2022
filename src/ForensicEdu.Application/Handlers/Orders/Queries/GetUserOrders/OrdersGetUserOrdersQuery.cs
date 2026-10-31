using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;

public sealed record OrdersGetUserOrdersQuery(string UserId) : IRequest<ApplicationResult>;
