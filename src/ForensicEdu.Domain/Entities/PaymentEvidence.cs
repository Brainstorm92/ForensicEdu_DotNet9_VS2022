using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class PaymentEvidence
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public Order? Order { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string OperationNumber { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string PaymentFingerprint { get; set; } = string.Empty;
    public string VoucherFileHash { get; set; } = string.Empty;
    public string VoucherSha256Hash { get; set; } = string.Empty;
    public string VoucherFileUrl { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDIENTE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidatedAt { get; set; }
    public string? ValidatedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}
