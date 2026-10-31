using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicAdvisorys;

public sealed class ListPublicAdvisorysQueryValidator : AbstractValidator<ListPublicAdvisorysQuery>
{
    public ListPublicAdvisorysQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
