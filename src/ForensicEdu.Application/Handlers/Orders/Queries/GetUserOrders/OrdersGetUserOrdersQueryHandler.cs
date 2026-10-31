using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;

public sealed class OrdersGetUserOrdersQueryHandler(IApplicationDbContext _context, ICurrentUser current) : IRequestHandler<OrdersGetUserOrdersQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(OrdersGetUserOrdersQuery message, CancellationToken cancellationToken)
    {
        var userId = message.UserId;

        if (userId != current.UserId) return ApplicationResult.Forbidden();
        var orders = await _context.Orders
             .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o=>new {o.Id,o.OrderCode,o.ProductId,o.ProductType,o.Amount,o.Price,o.Status,o.CreatedAt,payment=o.PaymentEvidence==null?null:new {o.PaymentEvidence.Id,o.PaymentEvidence.Status,o.PaymentEvidence.BankName,o.PaymentEvidence.OperationNumber,o.PaymentEvidence.RegisteredAt}})
            .ToListAsync(cancellationToken);

        return ApplicationResult.Success(orders);
    }
}
