using System;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfCertificateService _pdfCertificateService;

    public CertificatesController(IApplicationDbContext context, IPdfCertificateService pdfCertificateService)
    {
        _context = context;
        _pdfCertificateService = pdfCertificateService;
    }

    [HttpGet("verify/{code}")]
    public async Task<IActionResult> Verify(string code)
    {
        var cleanCode = code.Trim().ToUpperInvariant();
        var cert = await _context.DigitalCertificates
            .Include(c => c.Enrollment)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.CertificateCode == cleanCode);

        // Record verification audit
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var userAgent = Request.Headers.UserAgent.ToString();

        _context.CertificateVerificationLogs.Add(new CertificateVerificationLog
        {
            CertificateCode = cleanCode,
            IpAddress = ip,
            UserAgent = userAgent,
            IsValid = cert?.Status == ForensicEdu.Domain.Enums.CertificateStatus.VALIDO,
            VerifiedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        if (cert == null)
        {
            return NotFound(new
            {
                isValid = false,
                code = cleanCode,
                message = "El código de certificado ingresado no existe en los registros oficiales del Instituto Forense."
            });
        }

        return Ok(new
        {
            isValid = cert.Status == ForensicEdu.Domain.Enums.CertificateStatus.VALIDO,
            status = cert.Status.ToString(),
            code = cert.CertificateCode,
            student = cert.StudentFullName,
            course = cert.CourseTitle,
            hours = cert.AcademicHours,
            grade = cert.FinalGrade,
            issueDate = cert.IssueDate.ToString("dd/MM/yyyy"),
            revokedAt = cert.RevokedAt,
            revocationReason = cert.RevocationReason
        });
    }

    [HttpGet("download/{code}/pdf")]
    public async Task<IActionResult> DownloadPdf(string code)
    {
        var cert = await _context.DigitalCertificates.FirstOrDefaultAsync(c => c.CertificateCode == code.Trim().ToUpperInvariant());
        if (cert == null) return NotFound("Certificado no encontrado.");
        if (cert.Status != ForensicEdu.Domain.Enums.CertificateStatus.VALIDO)
            return Conflict(new { message = "El certificado no está vigente y no puede descargarse." });

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

        return File(pdfBytes, "application/pdf", $"Certificado_{cert.CertificateCode}.pdf");
    }
}
