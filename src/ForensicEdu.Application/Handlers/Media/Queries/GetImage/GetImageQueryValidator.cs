using FluentValidation;
namespace ForensicEdu.Application.Handlers.Media.Queries.GetImage;
public sealed class GetImageQueryValidator:AbstractValidator<GetImageQuery>
{
 public GetImageQueryValidator(){RuleFor(x=>x.Id).Must(x=>Guid.TryParseExact(x,"N",out _)).WithMessage("La imagen solicitada no es válida.");}
}
