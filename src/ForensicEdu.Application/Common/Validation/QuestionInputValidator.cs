using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class QuestionInputValidator : AbstractValidator<QuestionInput>
{
    public QuestionInputValidator() {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(100); RuleFor(x=>x.Question).NotEmpty().MaximumLength(5000);
        RuleFor(x=>x.Options).NotNull().Must(x=>x is { Length: >=2 and <=8 });
        When(x=>x.Options != null,()=>{ RuleForEach(x=>x.Options).NotEmpty().MaximumLength(2000);
            RuleFor(x=>x.CorrectOptionIndex).GreaterThanOrEqualTo(0).LessThan(x=>x.Options.Length); });
        RuleFor(x=>x.Explanation).NotNull().MaximumLength(5000);
    }
}
