using FluentValidation;
namespace ForensicEdu.Application.Handlers.Courses.Queries.GetById;

public sealed class CoursesGetByIdQueryValidator : AbstractValidator<CoursesGetByIdQuery>
{
    public CoursesGetByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
    }
}
