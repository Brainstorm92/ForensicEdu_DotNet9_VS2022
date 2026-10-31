using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class AdvisoryInputValidator : AbstractValidator<AdvisoryInput>
{
    public AdvisoryInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.Specialist).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Specialty).NotEmpty().MaximumLength(200); RuleFor(x=>x.Description).NotNull().MaximumLength(20000);
        RuleFor(x=>x.Price).InclusiveBetween(0,1000000).PrecisionScale(18,2,true); RuleFor(x=>x.WhatsappContact).NotEmpty().Matches("^\\+?[0-9]{7,15}$"); }
}
