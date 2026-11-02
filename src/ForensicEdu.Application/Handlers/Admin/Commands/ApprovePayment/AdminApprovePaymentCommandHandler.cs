using MediatR;
using ForensicEdu.Application.Services;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Admin.Commands.ApprovePayment;

public sealed class AdminApprovePaymentCommandHandler(IApplicationDbContext _context, ICurrentUser current, ICoursePurchaseGate gate) : IRequestHandler<AdminApprovePaymentCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AdminApprovePaymentCommand message, CancellationToken cancellationToken)
    {
        var order=await _context.PaymentEvidences.AsNoTracking().Where(p=>p.Id==message.PaymentId).Select(p=>p.Order).FirstOrDefaultAsync(cancellationToken);
        return order?.ProductType==ProductType.CURSO
            ? await gate.ExecuteAsync(order.UserId,order.ProductId,()=>Process(message,cancellationToken),cancellationToken)
            : await Process(message,cancellationToken);
    }
    private async Task<ApplicationResult> Process(AdminApprovePaymentCommand message, CancellationToken cancellationToken)
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

        List<Enrollment> previousEnrollments = [];
        if (payment.Order.ProductType == ProductType.CURSO)
        {
            if (await _context.Enrollments.AnyAsync(e=>e.OrderId==payment.OrderId,cancellationToken))
                return ApplicationResult.Conflict(new {message="La orden ya tiene una matrícula vinculada. Revise su estado antes de aprobar otro pago."});
            var eligibility=await CourseEnrollmentRules.CheckAsync(_context,payment.Order.UserId,payment.Order.ProductId,cancellationToken);
            if(!eligibility.Allowed)return ApplicationResult.Conflict(new {message=CourseEnrollmentRules.UnavailableMessage});
            previousEnrollments=eligibility.Previous;
        }

        payment.Status = PaymentStatus.APROBADO;
        payment.ValidatedAt = DateTime.UtcNow;
        payment.ValidatedByUserId = adminUserId;

        var order = payment.Order;
        if (order != null)
        {
            order.Status = OrderStatus.APROBADO;

            if (order.ProductType == ProductType.CURSO)
            {
                // A new paid cycle never overwrites the previous grades, attempts or materials.
                foreach(var old in previousEnrollments)old.Status="DESAPROBADA";
                var enrollment=new Enrollment {
                    UserId=order.UserId,CourseId=order.ProductId,OrderId=order.Id,
                    EnrolledAt=payment.ValidatedAt.Value,EnrollmentDate=payment.ValidatedAt.Value,
                    Status="ACTIVA",ExamStatus=ExamStatus.NO_INICIADA
                };
                _context.Enrollments.Add(enrollment);
                _context.AuditLogs.Add(new AuditLog {UserId=order.UserId,
                    Action=previousEnrollments.Count>0?"COURSE_REENROLLED":"COURSE_ENROLLED",
                    EntityType="Enrollment",EntityId=enrollment.Id,
                    Details=$"Matrícula vinculada a orden {order.OrderCode}. Ciclos anteriores: {string.Join(", ",previousEnrollments.Select(e=>e.Id))}."});
            }

            if(order.ProductType==ProductType.ASESORIA&&!await _context.AdvisoryCases.AnyAsync(c=>c.OrderId==order.Id,cancellationToken))
            {
                var care=new AdvisoryCase{OrderId=order.Id,AdvisoryServiceId=order.ProductId,StudentId=order.UserId};
                _context.AdvisoryCases.Add(care);
                _context.AdvisoryCareHistory.Add(new AdvisoryCareHistory{AdvisoryCaseId=care.Id,ActorId=adminUserId,Action="ADVISORY_CARE_CREATED",Details="Compra aprobada. Pendiente de asignación de consultor."});
            }
            // Extend 2-year account validity (Rule Francisco v5 Section 10)
            // Stage all changes in the same DbContext; one SaveChanges commits payment,
            // enrollment, account renewal and audit atomically in PostgreSQL.
            if (order.User == null) return ApplicationResult.NotFound(new { message = "Usuario no registrado." });
            order.User.RenewFromApprovedPayment(payment.ValidatedAt!.Value);
            await StudentNotifications.CancelValidityMail(_context, order.UserId, cancellationToken);
            var productName = string.IsNullOrWhiteSpace(order.ProductTitle) ? "tu compra" : order.ProductTitle;
            var title = order.ProductType == ProductType.CURSO ? (previousEnrollments.Count>0?"Tu nueva matrícula está confirmada":"Tu matrícula está confirmada") : "Tu compra está confirmada";
            var tab = order.ProductType == ProductType.CURSO ? "mis-cursos" : order.ProductType == ProductType.EVENTO ? "mis-eventos" : "asesoria";
            StudentNotifications.Add(_context, order.UserId, "payment:" + payment.Id, "MATRICULA", title,
                $"Aprobamos el pago de {productName}. Ya puedes consultar tu acceso en la plataforma. Tu cuenta tiene vigencia hasta el {order.User.ValidUntil:dd/MM/yyyy}." + (previousEnrollments.Count>0?" La nueva matrícula comienza con progreso cero y dos intentos nuevos.":""), tab);
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
