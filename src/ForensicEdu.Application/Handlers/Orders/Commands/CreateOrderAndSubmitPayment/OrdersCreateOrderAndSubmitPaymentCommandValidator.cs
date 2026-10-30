using FluentValidation;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Handlers.Orders.Commands.CreateOrderAndSubmitPayment;

public sealed class OrdersCreateOrderAndSubmitPaymentCommandValidator : AbstractValidator<OrdersCreateOrderAndSubmitPaymentCommand>
{
    public OrdersCreateOrderAndSubmitPaymentCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.UserId).NotEmpty();
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.ProductType).IsInEnum();
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.BankName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.OperationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.AccountHolderName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.OperationDate).Must(date => date != default && date <= DateTime.UtcNow).WithMessage("La fecha bancaria debe ser válida y no futura.");
        });
    }
}
