using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedEvent;

public sealed class GetManagedEventQueryValidator : AbstractValidator<GetManagedEventQuery>
{
    public GetManagedEventQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
