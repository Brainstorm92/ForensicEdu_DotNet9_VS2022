using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetClassroom;

public sealed class GetClassroomQueryValidator : AbstractValidator<GetClassroomQuery>
{
    public GetClassroomQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
