using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Handlers.Certificates.Commands.Verify;
public sealed record CertificatesVerifyCommand(string Code, string Ip, string UserAgent) : IRequest<ApplicationResult>;
public sealed class CertificatesVerifyCommandHandler(IApplicationDbContext _context) : IRequestHandler<CertificatesVerifyCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CertificatesVerifyCommand message, CancellationToken cancellationToken)
    {
        var code = message.Code;
        var ip = message.Ip;
        var userAgent = message.UserAgent;

        var cleanCode = code.Trim().ToUpperInvariant();
        var cert = await _context.DigitalCertificates
            .Include(c => c.Enrollment)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.CertificateCode == cleanCode);

        // Record verification audit

        _context.CertificateVerificationLogs.Add(new CertificateVerificationLog
        {
            CertificateCode = cleanCode,
            IpAddress = ip,
            UserAgent = userAgent,
            IsValid = cert?.Status == ForensicEdu.Domain.Enums.CertificateStatus.VALIDO,
            VerifiedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        if (cert == null)
        {
            return ApplicationResult.NotFound(new
            {
                isValid = false,
                code = cleanCode,
                message = "El código de certificado ingresado no existe en los registros oficiales del Instituto Forense."
            });
        }

        return ApplicationResult.Success(new
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
}

public sealed class CertificatesVerifyCommandValidator : AbstractValidator<CertificatesVerifyCommand>
{
    public CertificatesVerifyCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
    }
}
