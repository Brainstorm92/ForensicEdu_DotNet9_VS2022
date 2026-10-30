using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class CourseInputValidator : AbstractValidator<CourseInput>
{
    public CourseInputValidator() {
        RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x=>x.Category).NotEmpty().MaximumLength(100); RuleFor(x=>x.Price).InclusiveBetween(0,1000000).PrecisionScale(18,2,true);
        RuleFor(x=>x.DiscountPrice).InclusiveBetween(0,1000000).PrecisionScale(18,2,true).LessThanOrEqualTo(x=>x.Price);
        RuleFor(x=>x.AcademicHours).InclusiveBetween(1,10000); RuleFor(x=>x.Instructor).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ShortDescription).NotEmpty().MaximumLength(1000); RuleFor(x=>x.FullDescription).NotNull().MaximumLength(50000);
        RuleFor(x=>x.ShortCode).NotEmpty().MaximumLength(10).Matches("^[A-Za-z0-9-]+$");
        RuleFor(x=>x.ImageUrl).Must(InputUrls.ImageOrEmpty).WithMessage("Selecciona una portada o utiliza una URL HTTPS.");
        RuleFor(x=>x.InstructorImageUrl).Must(InputUrls.ImageOrEmpty).WithMessage("Selecciona una foto del docente o utiliza una URL HTTPS."); RuleFor(x=>x.Modalidad).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Requisitos).NotNull().MaximumLength(2000); RuleFor(x=>x.OrderIndex).InclusiveBetween(0,10000);
    }
}
