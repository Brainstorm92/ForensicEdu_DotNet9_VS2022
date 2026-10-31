using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;

public sealed class AdminRevokeCertificateCommandHandler(IApplicationDbContext _context, ICurrentUser current) : IRequestHandler<AdminRevokeCertificateCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AdminRevokeCertificateCommand message, CancellationToken cancellationToken)
    {
        var code = message.Code;
        var req = message.Req;

        var adminUserId = current.UserId!;
        var cert = await _context.DigitalCertificates.FirstOrDefaultAsync(c => c.CertificateCode == code.Trim().ToUpperInvariant());
        if (cert == null) return ApplicationResult.NotFound("Certificado no encontrado.");

        cert.Status = CertificateStatus.REVOCADO;
        cert.RevokedAt = DateTime.UtcNow;
        cert.RevocationReason = req.Reason;

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "CERTIFICATE_REVOKED",
            EntityType = "DigitalCertificate",
            EntityId = cert.Id,
            Details = $"Certificado {code} revocado. Causa: {req.Reason}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return ApplicationResult.Success(new { message = $"Certificado {code} revocado correctamente." });
    }
}
