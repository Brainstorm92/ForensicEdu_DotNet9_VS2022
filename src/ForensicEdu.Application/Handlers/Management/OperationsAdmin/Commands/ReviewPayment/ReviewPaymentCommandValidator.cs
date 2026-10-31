using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.ReviewPayment;

public sealed class ReviewPaymentCommandValidator : AbstractValidator<ReviewPaymentCommand>
{
    public ReviewPaymentCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new PaymentReviewInputValidator());
    }
}
