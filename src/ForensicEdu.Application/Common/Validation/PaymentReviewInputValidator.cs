using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class PaymentReviewInputValidator : AbstractValidator<PaymentReviewInput>
{
    public PaymentReviewInputValidator() { RuleFor(x=>x.Status).Must(s=>s is ForensicEdu.Domain.Enums.PaymentStatus.EN_REVISION or ForensicEdu.Domain.Enums.PaymentStatus.OBSERVADO or ForensicEdu.Domain.Enums.PaymentStatus.RECHAZADO);
        RuleFor(x=>x.Reason).NotEmpty().MaximumLength(2000); }
}
