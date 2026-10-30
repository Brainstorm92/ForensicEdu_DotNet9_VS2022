using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Admin;

namespace ForensicEdu.Application.Handlers.Admin.Commands.RevokeCertificate;
public sealed record AdminRevokeCertificateCommand(string Code, RevokeRequest Req) : IRequest<ApplicationResult>;
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

public sealed class AdminRevokeCertificateCommandValidator : AbstractValidator<AdminRevokeCertificateCommand>
{
    public AdminRevokeCertificateCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Req).NotNull();
        When(x => x.Req != null, () => { RuleFor(x => x.Req.Reason).NotEmpty().MaximumLength(2000); });
    }
}
