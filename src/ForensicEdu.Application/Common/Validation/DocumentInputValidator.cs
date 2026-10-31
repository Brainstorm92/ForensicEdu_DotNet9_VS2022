using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class DocumentInputValidator : AbstractValidator<DocumentInput>
{
    public DocumentInputValidator() { RuleFor(x=>x.Title).NotEmpty().MaximumLength(300); RuleFor(x=>x.StorageUrl).NotEmpty().Must(InputUrls.HttpsOrEmpty);
        RuleFor(x=>x.FileType).Must(x=>x is "PDF" or "DOCX" or "PPTX" or "XLSX"); RuleFor(x=>x.FileSize).NotNull().MaximumLength(50);
        RuleFor(x=>x.RequireWatermark).Equal(false).WithMessage("La generación de marcas de agua aún no está integrada."); }
}
