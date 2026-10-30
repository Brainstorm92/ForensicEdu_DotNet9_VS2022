using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ForensicEdu.Application.Services;

public class AntifraudService : IAntifraudService
{
    private readonly IApplicationDbContext _context;

    public AntifraudService(IApplicationDbContext context)
    {
        _context = context;
    }

    public string GenerateFingerprint(string bank, string operationNumber, DateTime date, decimal amount, string accountHolder)
    {
        // Normalization matching Francisco v5 Section 9:
        // banco + num_operacion + fecha + monto + titular
        var cleanBank = bank.Trim().ToUpperInvariant();
        var cleanOp = operationNumber.Trim().ToUpperInvariant();
        var cleanDate = date.ToString("yyyy-MM-dd");
        var cleanAmount = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        var cleanHolder = accountHolder.Trim().ToUpperInvariant();

        return $"{cleanBank}|{cleanOp}|{cleanDate}|{cleanAmount}|{cleanHolder}";
    }

    public string ComputeVoucherSha256(byte[] voucherBytes)
    {
        if (voucherBytes == null || voucherBytes.Length == 0) return string.Empty;
        var hash = SHA256.HashData(voucherBytes);
        return Convert.ToHexStringLower(hash);
    }

    public string ComputeVoucherSha256(Stream voucherStream)
    {
        if (voucherStream == null) return string.Empty;
        voucherStream.Position = 0;
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(voucherStream);
        return Convert.ToHexStringLower(hash);
    }

    public async Task<AntifraudEvaluationResult> EvaluatePaymentAsync(
        string bank,
        string operationNumber,
        DateTime date,
        decimal amount,
        string accountHolder,
        byte[]? voucherBytes = null,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = GenerateFingerprint(bank, operationNumber, date, amount, accountHolder);
        var voucherHash = voucherBytes != null ? ComputeVoucherSha256(voucherBytes) : string.Empty;

        // Check 1: Fingerprint match against already approved payments
        var duplicateFingerprint = await _context.PaymentEvidences
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => (p.PaymentFingerprint == fingerprint ||
                (p.BankName == bank.Trim().ToUpperInvariant() && p.OperationNumber == operationNumber.Trim().ToUpperInvariant()))
                && p.Status == PaymentStatus.APROBADO, cancellationToken);

        if (duplicateFingerprint != null)
        {
            return new AntifraudEvaluationResult(
                IsDuplicate: true,
                Fingerprint: fingerprint,
                VoucherHash: voucherHash,
                ConflictingPaymentId: duplicateFingerprint.Id,
                ConflictingOrderCode: duplicateFingerprint.Order?.OrderCode ?? duplicateFingerprint.OrderId,
                Message: $"ALERTA ANTIFRAUDE: Huella bancaria duplicada. Esta operación ya fue aprobada en la orden '{duplicateFingerprint.Order?.OrderCode}' por el titular '{duplicateFingerprint.AccountHolderName}'."
            );
        }

        // Check 2: Voucher SHA-256 collision check
        if (!string.IsNullOrEmpty(voucherHash))
        {
            var duplicateVoucher = await _context.PaymentEvidences
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.VoucherFileHash == voucherHash && p.Status == PaymentStatus.APROBADO, cancellationToken);

            if (duplicateVoucher != null)
            {
                return new AntifraudEvaluationResult(
                    IsDuplicate: true,
                    Fingerprint: fingerprint,
                    VoucherHash: voucherHash,
                    ConflictingPaymentId: duplicateVoucher.Id,
                    ConflictingOrderCode: duplicateVoucher.Order?.OrderCode ?? duplicateVoucher.OrderId,
                    Message: $"ALERTA ANTIFRAUDE: Archivo comprobante idéntico detectado (SHA-256 colisiona con orden '{duplicateVoucher.Order?.OrderCode}')."
                );
            }
        }

        return new AntifraudEvaluationResult(
            IsDuplicate: false,
            Fingerprint: fingerprint,
            VoucherHash: voucherHash,
            ConflictingPaymentId: null,
            ConflictingOrderCode: null,
            Message: "Comprobante válido y único."
        );
    }
}
