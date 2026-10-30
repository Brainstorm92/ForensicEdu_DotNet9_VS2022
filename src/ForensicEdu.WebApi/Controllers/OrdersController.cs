using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.IO;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAntifraudService _antifraudService;

    public OrdersController(IApplicationDbContext context, IAntifraudService antifraudService)
    {
        _context = context;
        _antifraudService = antifraudService;
    }

    public record CreateOrderRequest(
        string UserId,
        string ProductId,
        ProductType ProductType,
        decimal Amount,
        string BankName,
        string OperationNumber,
        DateTime OperationDate,
        string AccountHolderName
    );

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateOrderAndSubmitPayment([FromBody] CreateOrderRequest request)
    {
        if (request.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null) return NotFound(new { message = "Usuario no registrado." });
        if (string.IsNullOrWhiteSpace(request.BankName) || string.IsNullOrWhiteSpace(request.OperationNumber) ||
            string.IsNullOrWhiteSpace(request.AccountHolderName) || request.OperationDate > DateTime.UtcNow || request.Amount <= 0)
            return BadRequest(new { message = "Complete los datos bancarios con un monto positivo y una fecha válida." });
        decimal? price = request.ProductType switch
        {
            ProductType.CURSO => await _context.Courses.Where(c => c.Id == request.ProductId && c.IsPublished && c.IsActive)
                .Select(c => (decimal?)(c.DiscountPrice > 0 ? c.DiscountPrice : c.Price)).FirstOrDefaultAsync(),
            ProductType.EVENTO => await _context.LiveEvents.Where(e => e.Id == request.ProductId).Select(e => (decimal?)e.Price).FirstOrDefaultAsync(),
            ProductType.ASESORIA => await _context.AdvisoryServices.Where(a => a.Id == request.ProductId).Select(a => (decimal?)a.Price).FirstOrDefaultAsync(),
            _ => null
        };
        if (price == null) return NotFound(new { message = "Producto no disponible." });
        if (price.Value != request.Amount) return BadRequest(new { message = "El monto no coincide con el precio vigente del producto." });

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
            return BadRequest(new
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

        var order = new Order
        {
            OrderCode = orderCode,
            UserId = user.Id,
            ProductId = request.ProductId,
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

        await _context.SaveChangesAsync();

        return Ok(new
        {
            orderId = order.Id,
            orderCode = order.OrderCode,
            status = order.Status.ToString(),
            message = "Comprobante recibido con éxito. Su pago pasará a validación contable y antifraude."
        });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(string userId)
    {
        if (userId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
        var orders = await _context.Orders
            .Include(o => o.PaymentEvidence)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost("{orderId}/voucher")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadVoucher(string orderId, IFormFile file,
        [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var payment = await _context.PaymentEvidences.Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Order!.UserId == userId);
        if (payment == null) return NotFound();
        if (payment.Status == PaymentStatus.APROBADO) return Conflict();
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024) return BadRequest(new { message = "Adjunte JPG, PNG o PDF de hasta 5 MB." });
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var bytes = stream.ToArray();
        var extension = bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D ? ".pdf"
            : bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) ? ".png"
            : bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF ? ".jpg" : null;
        if (extension == null) return BadRequest(new { message = "Formato no permitido." });
        var hash = _antifraudService.ComputeVoucherSha256(bytes);
        if (await _context.PaymentEvidences.AnyAsync(p => p.Id != payment.Id && p.VoucherFileHash == hash && p.Status == PaymentStatus.APROBADO))
            return Conflict(new { message = "Este comprobante ya fue utilizado." });
        var folder = Path.Combine(environment.ContentRootPath, "App_Data", "vouchers");
        Directory.CreateDirectory(folder);
        var storageName = Guid.NewGuid().ToString("N") + extension;
        await System.IO.File.WriteAllBytesAsync(Path.Combine(folder, storageName), bytes);
        payment.VoucherFileUrl = storageName;
        payment.VoucherFileHash = hash;
        payment.VoucherSha256Hash = hash;
        _context.AuditLogs.Add(new AuditLog { UserId = userId!, Action = "VOUCHER_UPLOADED", EntityType = "PaymentEvidence", EntityId = payment.Id });
        await _context.SaveChangesAsync();
        return Ok(new { message = "Comprobante guardado en almacenamiento privado." });
    }
}
