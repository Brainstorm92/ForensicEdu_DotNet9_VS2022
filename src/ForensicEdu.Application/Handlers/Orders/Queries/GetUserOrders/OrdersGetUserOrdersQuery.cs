using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Orders;

namespace ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;
public sealed record OrdersGetUserOrdersQuery(string UserId) : IRequest<ApplicationResult>;
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

public sealed class OrdersGetUserOrdersQueryValidator : AbstractValidator<OrdersGetUserOrdersQuery>
{
    public OrdersGetUserOrdersQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(200);
    }
}
