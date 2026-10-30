using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListAcademicAttempts;

public sealed class ListAcademicAttemptsQueryValidator : AbstractValidator<ListAcademicAttemptsQuery>
{
    public ListAcademicAttemptsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
