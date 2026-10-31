using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPayment;

public sealed class GetPaymentQueryValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
