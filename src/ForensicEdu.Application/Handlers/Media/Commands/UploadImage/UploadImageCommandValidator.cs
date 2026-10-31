using FluentValidation;
namespace ForensicEdu.Application.Handlers.Media.Commands.UploadImage;
public sealed class UploadImageCommandValidator:AbstractValidator<UploadImageCommand>
{
 public UploadImageCommandValidator(){RuleFor(x=>x.Bytes).NotNull().Must(b=>b is {Length:>0 and <=5242880}).WithMessage("Selecciona una imagen de hasta 5 MB.");}
}
