using FluentValidation;
namespace ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;

public sealed class AdminApprovePaymentCommandValidator : AbstractValidator<AdminApprovePaymentCommand>
{
    public AdminApprovePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty().MaximumLength(200);
    }
}
