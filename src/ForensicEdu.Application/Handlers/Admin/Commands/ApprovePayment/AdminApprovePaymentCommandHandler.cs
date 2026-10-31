using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;

public sealed class AdminApprovePaymentCommandHandler(IApplicationDbContext _context, ICurrentUser current) : IRequestHandler<AdminApprovePaymentCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AdminApprovePaymentCommand message, CancellationToken cancellationToken)
    {
        var paymentId = message.PaymentId;

        var adminUserId = current.UserId!;
        var payment = await _context.PaymentEvidences
            .Include(p => p.Order)
                .ThenInclude(o => o!.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return ApplicationResult.NotFound(new { message = "Comprobante no encontrado." });
        if (payment.Status == PaymentStatus.APROBADO) return ApplicationResult.Invalid(new { message = "Este comprobante ya fue aprobado." });
        if (payment.Order == null || payment.Status == PaymentStatus.DUPLICADO || payment.Status == PaymentStatus.RECHAZADO)
            return ApplicationResult.Invalid(new { message = "El pago no está habilitado para aprobación." });
        if (payment.Amount != payment.Order.Amount)
            return ApplicationResult.Invalid(new { message = "El monto no corresponde a la orden." });
        if (string.IsNullOrWhiteSpace(payment.VoucherFileHash))
            return ApplicationResult.Invalid(new { message = "Debe adjuntar un comprobante antes de aprobar el pago." });
        if (await _context.PaymentEvidences.AnyAsync(p => p.Id != payment.Id && p.Status == PaymentStatus.APROBADO &&
            ((p.BankName == payment.BankName && p.OperationNumber == payment.OperationNumber) ||
             p.PaymentFingerprint == payment.PaymentFingerprint ||
             (payment.VoucherFileHash != "" && p.VoucherFileHash == payment.VoucherFileHash))))
        {
            payment.Status = PaymentStatus.DUPLICADO;
            _context.AuditLogs.Add(new AuditLog { UserId = adminUserId, Action = "PAYMENT_DUPLICATE", EntityType = "PaymentEvidence", EntityId = payment.Id });
            await _context.SaveChangesAsync(cancellationToken);
            return ApplicationResult.Conflict(new { message = "La operación o el comprobante ya se utilizó en un pago aprobado." });
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
            // Stage all changes in the same DbContext; one SaveChanges commits payment,
            // enrollment, account renewal and audit atomically in PostgreSQL.
            if (order.User == null) return ApplicationResult.NotFound(new { message = "Usuario no registrado." });
            order.User.RenewFromApprovedPayment(payment.ValidatedAt!.Value);
            _context.AuditLogs.Add(new AuditLog { UserId = order.UserId, Action = "ACCOUNT_VALIDITY_EXTENDED",
                EntityType = "User", EntityId = order.UserId, Details = "Vigencia renovada por pago aprobado." });
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

        await _context.SaveChangesAsync(cancellationToken);

        return ApplicationResult.Success(new { message = "Pago aprobado exitosamente. Matrícula y vigencia de 2 años actualizadas." });
    }
}
