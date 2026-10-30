using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicEvent;

public sealed class GetPublicEventQueryValidator : AbstractValidator<GetPublicEventQuery>
{
    public GetPublicEventQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
