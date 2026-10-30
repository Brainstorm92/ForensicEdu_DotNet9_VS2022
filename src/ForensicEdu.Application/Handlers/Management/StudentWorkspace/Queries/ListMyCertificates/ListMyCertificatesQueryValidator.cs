using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.ListMyCertificates;

public sealed class ListMyCertificatesQueryValidator : AbstractValidator<ListMyCertificatesQuery>
{
    public ListMyCertificatesQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
