using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedAdvisory;

public sealed class GetManagedAdvisoryQueryValidator : AbstractValidator<GetManagedAdvisoryQuery>
{
    public GetManagedAdvisoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
