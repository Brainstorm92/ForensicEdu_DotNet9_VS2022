using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPaymentVoucher;

public sealed class GetPaymentVoucherQueryValidator : AbstractValidator<GetPaymentVoucherQuery>
{
    public GetPaymentVoucherQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
