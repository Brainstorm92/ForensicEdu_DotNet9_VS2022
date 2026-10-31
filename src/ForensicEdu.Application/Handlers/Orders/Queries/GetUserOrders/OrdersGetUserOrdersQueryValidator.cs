using FluentValidation;
namespace ForensicEdu.Application.Handlers.Orders.Queries.GetUserOrders;

public sealed class OrdersGetUserOrdersQueryValidator : AbstractValidator<OrdersGetUserOrdersQuery>
{
    public OrdersGetUserOrdersQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(200);
    }
}
