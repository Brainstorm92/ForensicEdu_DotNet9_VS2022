using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[Authorize(Roles = "1,2,3")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAccountLifecycleService _accountLifecycleService;

    public AdminController(IApplicationDbContext context, IAccountLifecycleService accountLifecycleService)
    {
        _context = context;
        _accountLifecycleService = accountLifecycleService;
    }

    [HttpGet("pending-payments")]
    public async Task<IActionResult> GetPendingPayments()
    {
        var pending = await _context.PaymentEvidences
            .Include(p => p.Order)
                .ThenInclude(o => o!.User)
            .Where(p => p.Status == PaymentStatus.PENDIENTE)
            .OrderByDescending(p => p.RegisteredAt)
            .ToListAsync();

        return Ok(pending);
    }

    [Authorize(Roles = "1,3")]
    [HttpPost("payments/{paymentId}/approve")]
    public async Task<IActionResult> ApprovePayment(string paymentId, [FromQuery] string? adminUserId = null)
    {
        adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var payment = await _context.PaymentEvidences
            .Include(p => p.Order)
                .ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return NotFound(new { message = "Comprobante no encontrado." });
        if (payment.Status == PaymentStatus.APROBADO) return BadRequest(new { message = "Este comprobante ya fue aprobado." });
        if (payment.Order == null || payment.Status == PaymentStatus.DUPLICADO || payment.Status == PaymentStatus.RECHAZADO)
            return BadRequest(new { message = "El pago no está habilitado para aprobación." });
        if (payment.Amount != payment.Order.Amount)
            return BadRequest(new { message = "El monto no corresponde a la orden." });
        if (string.IsNullOrWhiteSpace(payment.VoucherFileHash))
            return BadRequest(new { message = "Debe adjuntar un comprobante antes de aprobar el pago." });
        if (await _context.PaymentEvidences.AnyAsync(p => p.Id != payment.Id && p.Status == PaymentStatus.APROBADO &&
            ((p.BankName == payment.BankName && p.OperationNumber == payment.OperationNumber) ||
             p.PaymentFingerprint == payment.PaymentFingerprint ||
             (payment.VoucherFileHash != "" && p.VoucherFileHash == payment.VoucherFileHash))))
        {
            payment.Status = PaymentStatus.DUPLICADO;
            _context.AuditLogs.Add(new AuditLog { UserId = adminUserId, Action = "PAYMENT_DUPLICATE", EntityType = "PaymentEvidence", EntityId = payment.Id });
            await _context.SaveChangesAsync();
            return Conflict(new { message = "La operación o el comprobante ya se utilizó en un pago aprobado." });
        }

        payment.Status = PaymentStatus.APROBADO;
        payment.ValidatedAt = DateTime.UtcNow;
        payment.ValidatedByUserId = adminUserId;

        var order = payment.Order;
        if (order != null)
        {
            order.Status = OrderStatus.APROBADO;

            // Activate Enrollment if Course
            if (order.ProductType == ProductType.CURSO)
            {
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == order.UserId && e.CourseId == order.ProductId);

                if (existingEnrollment == null)
                {
                    _context.Enrollments.Add(new Enrollment
                    {
                        UserId = order.UserId,
                        CourseId = order.ProductId,
                        OrderId = order.Id,
                        EnrolledAt = DateTime.UtcNow,
                        ExamStatus = ExamStatus.NO_INICIADA,
                        ProgressPercent = 0
                    });
                }
            }

            // Extend 2-year account validity (Rule Francisco v5 Section 10)
            await _accountLifecycleService.ExtendValidityOnApprovedPaymentAsync(order.UserId, payment.ValidatedAt!.Value);
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = adminUserId,
            Action = "PAYMENT_APPROVED",
            EntityType = "PaymentEvidence",
            EntityId = payment.Id,
            Details = $"Pago aprobado para orden {payment.Order?.OrderCode}. Matrícula habilitada y vigencia de 2 años renovada.",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { message = "Pago aprobado exitosamente. Matrícula y vigencia de 2 años actualizadas." });
    }

    [Authorize(Roles = "1,2")]
    [HttpPost("certificates/{code}/revoke")]
    public async Task<IActionResult> RevokeCertificate(string code, [FromBody] RevokeRequest req, [FromQuery] string? adminUserId = null)
    {
        adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cert = await _context.DigitalCertificates.FirstOrDefaultAsync(c => c.CertificateCode == code.Trim().ToUpperInvariant());
        if (cert == null) return NotFound("Certificado no encontrado.");

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

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Certificado {code} revocado correctamente." });
    }

    public record RevokeRequest(string Reason);

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 50)
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return Ok(logs);
    }
}
