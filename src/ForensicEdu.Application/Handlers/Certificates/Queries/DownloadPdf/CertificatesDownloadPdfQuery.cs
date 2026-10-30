using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.Certificates.Queries.DownloadPdf;
public sealed record CertificatesDownloadPdfQuery(string Code) : IRequest<ApplicationResult>;
public sealed class CertificatesDownloadPdfQueryHandler(IApplicationDbContext _context, IPdfCertificateService _pdfCertificateService, ICurrentUser current) : IRequestHandler<CertificatesDownloadPdfQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CertificatesDownloadPdfQuery message, CancellationToken cancellationToken)
    {
        var code = message.Code;

        var cert = await _context.DigitalCertificates.FirstOrDefaultAsync(c => c.CertificateCode == code.Trim().ToUpperInvariant());
        if (cert == null) return ApplicationResult.NotFound("Certificado no encontrado.");
        if(cert.UserId!=current.UserId && !current.IsInRole("1") && !current.IsInRole("2"))return ApplicationResult.Forbidden();
        if (cert.Status != ForensicEdu.Domain.Enums.CertificateStatus.VALIDO)
            return ApplicationResult.Conflict(new { message = "El certificado no está vigente y no puede descargarse." });

        var pdfBytes = await _pdfCertificateService.GenerateCertificatePdfAsync(
            cert.StudentFullName,
            cert.StudentDni,
            cert.CourseTitle,
            cert.AcademicHours,
            cert.CertificateCode,
            cert.QrValidationUrl,
            cert.IssueDate,
            cert.FinalGrade
        );

        return ApplicationResult.Download(pdfBytes, "application/pdf", $"Certificado_{cert.CertificateCode}.pdf");
    }
}

public sealed class CertificatesDownloadPdfQueryValidator : AbstractValidator<CertificatesDownloadPdfQuery>
{
    public CertificatesDownloadPdfQueryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
    }
}
