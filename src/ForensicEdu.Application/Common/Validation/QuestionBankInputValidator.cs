using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class QuestionBankInputValidator : AbstractValidator<QuestionBankInput>
{
    public QuestionBankInputValidator() {
        RuleFor(x=>x.Questions).NotNull().Must(q=>q is { Length: >0 and <=500 });
        When(x=>x.Questions != null,()=>{ RuleForEach(x=>x.Questions).NotNull().SetValidator(new QuestionInputValidator());
            RuleFor(x=>x.Questions).Must(q=>q.All(x=>x != null) && q.Select(x=>x.Id).Distinct().Count()==q.Length).WithMessage("Los identificadores de preguntas deben ser únicos."); });
    }
}
