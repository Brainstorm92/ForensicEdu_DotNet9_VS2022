using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.SaveQuestionBank;

public sealed class SaveQuestionBankCommandValidator : AbstractValidator<SaveQuestionBankCommand>
{
    public SaveQuestionBankCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new QuestionBankInputValidator());
    }
}
