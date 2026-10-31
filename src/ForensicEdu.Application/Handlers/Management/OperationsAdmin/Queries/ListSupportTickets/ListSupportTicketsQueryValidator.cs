using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListSupportTickets;

public sealed class ListSupportTicketsQueryValidator : AbstractValidator<ListSupportTicketsQuery>
{
    public ListSupportTicketsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
        RuleFor(x=>x.Status).IsInEnum().When(x=>x.Status.HasValue);
    }
}
