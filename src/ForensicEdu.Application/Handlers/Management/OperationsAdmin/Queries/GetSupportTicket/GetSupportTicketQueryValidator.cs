using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSupportTicket;

public sealed class GetSupportTicketQueryValidator : AbstractValidator<GetSupportTicketQuery>
{
    public GetSupportTicketQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
