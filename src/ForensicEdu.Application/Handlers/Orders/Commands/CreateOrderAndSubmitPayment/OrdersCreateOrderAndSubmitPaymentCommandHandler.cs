using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Orders.Commands.CreateOrderAndSubmitPayment;

public sealed class OrdersCreateOrderAndSubmitPaymentCommandHandler(IApplicationDbContext _context, IAntifraudService _antifraudService, ICurrentUser current) : IRequestHandler<OrdersCreateOrderAndSubmitPaymentCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(OrdersCreateOrderAndSubmitPaymentCommand message, CancellationToken cancellationToken)
    {
        var request = message.Request;

        if (request.UserId != current.UserId) return ApplicationResult.Forbidden();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null) return ApplicationResult.NotFound(new { message = "Usuario no registrado." });
        if (string.IsNullOrWhiteSpace(request.BankName) || string.IsNullOrWhiteSpace(request.OperationNumber) ||
            string.IsNullOrWhiteSpace(request.AccountHolderName) || request.OperationDate > DateTime.UtcNow || request.Amount <= 0)
            return ApplicationResult.Invalid(new { message = "Complete los datos bancarios con un monto positivo y una fecha válida." });
        decimal? price = request.ProductType switch
        {
            ProductType.CURSO => await _context.Courses.Where(c => c.Id == request.ProductId && c.IsPublished && c.IsActive)
                .Select(c => (decimal?)(c.DiscountPrice > 0 ? c.DiscountPrice : c.Price)).FirstOrDefaultAsync(),
            ProductType.EVENTO => await _context.LiveEvents.Where(e => e.Id == request.ProductId).Select(e => (decimal?)e.Price).FirstOrDefaultAsync(),
            ProductType.ASESORIA => await _context.AdvisoryServices.Where(a => a.Id == request.ProductId).Select(a => (decimal?)a.Price).FirstOrDefaultAsync(),
            _ => null
        };
        if (price == null) return ApplicationResult.NotFound(new { message = "Producto no disponible." });
        if (price.Value != request.Amount) return ApplicationResult.Invalid(new { message = "El monto no coincide con el precio vigente del producto." });

        var bank=request.BankName.Trim().ToUpperInvariant();var operation=request.OperationNumber.Trim().ToUpperInvariant();
        var pending=await _context.PaymentEvidences.Include(p=>p.Order).FirstOrDefaultAsync(p=>p.BankName==bank && p.OperationNumber==operation &&
            (p.Status==PaymentStatus.PENDIENTE || p.Status==PaymentStatus.EN_REVISION || p.Status==PaymentStatus.OBSERVADO),cancellationToken);
        if(pending!=null){
            if(pending.Order?.UserId==user.Id && pending.Order.ProductId==request.ProductId && pending.Order.ProductType==request.ProductType && pending.Amount==request.Amount)
                return ApplicationResult.Success(new {orderId=pending.Order.Id,orderCode=pending.Order.OrderCode,status=pending.Order.Status.ToString(),message="Se reutiliza la orden pendiente de esta misma operación."});
            return ApplicationResult.Conflict(new {message="La operación bancaria ya está vinculada a otra orden pendiente."});
        }
        // Rule 1: Evaluate Antifraud Fingerprint
        var antifraudCheck = await _antifraudService.EvaluatePaymentAsync(
            request.BankName,
            request.OperationNumber,
            request.OperationDate,
            request.Amount,
            request.AccountHolderName
        );

        if (antifraudCheck.IsDuplicate)
        {
            return ApplicationResult.Invalid(new
            {
                isFraud = true,
                message = antifraudCheck.Message,
                conflictingOrderCode = antifraudCheck.ConflictingOrderCode
            });
        }

        // Rule 2: Generate unique Order code ORD-YYYY-XXXXXX (Section 8)
        var year = DateTime.UtcNow.Year;
        var orderNumber = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var orderCode = $"ORD-{year}-{orderNumber}";

        var productTitle=request.ProductType switch {
            ProductType.CURSO=>await _context.Courses.Where(x=>x.Id==request.ProductId).Select(x=>x.Title).FirstAsync(cancellationToken),
            ProductType.EVENTO=>await _context.LiveEvents.Where(x=>x.Id==request.ProductId).Select(x=>x.Title).FirstAsync(cancellationToken),
            _=>await _context.AdvisoryServices.Where(x=>x.Id==request.ProductId).Select(x=>x.Title).FirstAsync(cancellationToken)
        };
        var order = new Order
        {
            OrderCode = orderCode,
            UserId = user.Id,
            ProductId = request.ProductId,
            ProductTitle = productTitle,
            ProductType = request.ProductType,
            Amount = request.Amount,
            Price = price.Value,
            Status = OrderStatus.PENDIENTE_VALIDACION,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);

        var payment = new PaymentEvidence
        {
            OrderId = order.Id,
            BankName = request.BankName.Trim().ToUpperInvariant(),
            OperationNumber = request.OperationNumber.Trim().ToUpperInvariant(),
            OperationDate = request.OperationDate,
            Amount = request.Amount,
            AccountHolderName = request.AccountHolderName.Trim().ToUpperInvariant(),
            PaymentFingerprint = antifraudCheck.Fingerprint,
            VoucherFileHash = antifraudCheck.VoucherHash,
            Status = PaymentStatus.PENDIENTE,
            RegisteredAt = DateTime.UtcNow
        };

        _context.PaymentEvidences.Add(payment);

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "ORDER_AND_PAYMENT_SUBMITTED",
            EntityType = "Order",
            EntityId = order.Id,
            Details = $"Orden {orderCode} registrada por S/ {request.Amount:F2}. Huella bancaria antifraude registrada para validación contable.",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return ApplicationResult.Success(new
        {
            orderId = order.Id,
            orderCode = order.OrderCode,
            status = order.Status.ToString(),
            message = "Comprobante recibido con éxito. Su pago pasará a validación contable y antifraude."
        });
    }
}
