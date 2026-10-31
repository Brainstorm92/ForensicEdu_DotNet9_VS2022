using FluentValidation;
namespace ForensicEdu.Application.Handlers.Orders.Commands.UploadVoucher;

public sealed class OrdersUploadVoucherCommandValidator : AbstractValidator<OrdersUploadVoucherCommand>
{
    public OrdersUploadVoucherCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Bytes).NotNull().Must(b => b is { Length: > 0 and <= 5242880 }).WithMessage("Adjunte un comprobante de hasta 5 MB.");
    }
}
