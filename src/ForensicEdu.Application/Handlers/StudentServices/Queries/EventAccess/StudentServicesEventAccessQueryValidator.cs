using FluentValidation;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.EventAccess;

public sealed class StudentServicesEventAccessQueryValidator : AbstractValidator<StudentServicesEventAccessQuery>
{
    public StudentServicesEventAccessQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
    }
}
