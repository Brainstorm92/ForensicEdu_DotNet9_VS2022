using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyPayments;

public sealed class ListMyPaymentsQueryValidator : AbstractValidator<ListMyPaymentsQuery>
{
    public ListMyPaymentsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
