using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedCourse;

public sealed class GetManagedCourseQueryValidator : AbstractValidator<GetManagedCourseQuery>
{
    public GetManagedCourseQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
