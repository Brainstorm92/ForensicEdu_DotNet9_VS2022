using FluentValidation;
namespace ForensicEdu.Application.Handlers.Certificates.Queries.DownloadPdf;

public sealed class CertificatesDownloadPdfQueryValidator : AbstractValidator<CertificatesDownloadPdfQuery>
{
    public CertificatesDownloadPdfQueryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
    }
}
