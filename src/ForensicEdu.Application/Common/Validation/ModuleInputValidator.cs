using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class ModuleInputValidator : AbstractValidator<ModuleInput>
{
    public ModuleInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.OrderIndex).InclusiveBetween(0,10000);
        RuleFor(x=>x.Duration).NotNull().MaximumLength(100); RuleFor(x=>x.VideoStreamingUrl).MaximumLength(2048).Must(InputUrls.HttpsOrEmpty); }
}
