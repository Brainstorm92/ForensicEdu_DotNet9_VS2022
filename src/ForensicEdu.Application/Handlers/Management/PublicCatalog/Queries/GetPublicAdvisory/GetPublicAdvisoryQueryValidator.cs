using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicAdvisory;

public sealed class GetPublicAdvisoryQueryValidator : AbstractValidator<GetPublicAdvisoryQuery>
{
    public GetPublicAdvisoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
