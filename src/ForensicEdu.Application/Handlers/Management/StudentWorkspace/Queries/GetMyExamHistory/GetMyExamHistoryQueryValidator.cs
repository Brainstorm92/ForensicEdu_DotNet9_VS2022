using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetMyExamHistory;

public sealed class GetMyExamHistoryQueryValidator : AbstractValidator<GetMyExamHistoryQuery>
{
    public GetMyExamHistoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
