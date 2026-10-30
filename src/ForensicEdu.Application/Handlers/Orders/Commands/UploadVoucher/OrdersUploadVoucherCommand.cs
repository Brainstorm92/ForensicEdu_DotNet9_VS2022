using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Orders;

namespace ForensicEdu.Application.Handlers.Orders.Commands.UploadVoucher;
public sealed record OrdersUploadVoucherCommand(string OrderId, byte[] Bytes) : IRequest<ApplicationResult>;
public sealed class OrdersUploadVoucherCommandHandler(IApplicationDbContext _context, IAntifraudService _antifraudService, ICurrentUser current, IVoucherStorage storage) : IRequestHandler<OrdersUploadVoucherCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(OrdersUploadVoucherCommand message, CancellationToken cancellationToken)
    {
        var orderId = message.OrderId;
        var bytes = message.Bytes;

        var userId = current.UserId;
        var payment = await _context.PaymentEvidences.Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Order!.UserId == userId);
        if (payment == null) return ApplicationResult.NotFound();
        if (payment.Status == PaymentStatus.APROBADO) return ApplicationResult.Conflict();
        if (bytes.Length == 0 || bytes.Length > 5 * 1024 * 1024) return ApplicationResult.Invalid(new { message = "Adjunte JPG, PNG o PDF de hasta 5 MB." });

        var extension = bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D ? ".pdf"
            : bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) ? ".png"
            : bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF ? ".jpg" : null;
        if (extension == null) return ApplicationResult.Invalid(new { message = "Formato no permitido." });
        var hash = _antifraudService.ComputeVoucherSha256(bytes);
        if (await _context.PaymentEvidences.AnyAsync(p => p.Id != payment.Id && p.VoucherFileHash == hash && p.Status == PaymentStatus.APROBADO))
            return ApplicationResult.Conflict(new { message = "Este comprobante ya fue utilizado." });
        var storageName = await storage.SaveAsync(bytes, extension, cancellationToken);
        payment.VoucherFileUrl = storageName;
        payment.VoucherFileHash = hash;
        payment.VoucherSha256Hash = hash;
        _context.AuditLogs.Add(new AuditLog { UserId = userId!, Action = "VOUCHER_UPLOADED", EntityType = "PaymentEvidence", EntityId = payment.Id });
        await _context.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success(new { message = "Comprobante guardado en almacenamiento privado." });
    }
}

public sealed class OrdersUploadVoucherCommandValidator : AbstractValidator<OrdersUploadVoucherCommand>
{
    public OrdersUploadVoucherCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Bytes).NotNull().Must(b => b is { Length: > 0 and <= 5242880 }).WithMessage("Adjunte un comprobante de hasta 5 MB.");
    }
}
