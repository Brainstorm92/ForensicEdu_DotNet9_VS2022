using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetQuestionBank;

public sealed class GetQuestionBankQueryValidator : AbstractValidator<GetQuestionBankQuery>
{
    public GetQuestionBankQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
