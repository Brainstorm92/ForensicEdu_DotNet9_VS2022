using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedAdvisorys;

public sealed class ListManagedAdvisorysQueryValidator : AbstractValidator<ListManagedAdvisorysQuery>
{
    public ListManagedAdvisorysQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
