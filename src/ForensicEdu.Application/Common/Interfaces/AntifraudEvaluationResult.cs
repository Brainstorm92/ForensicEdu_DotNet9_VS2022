using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public record AntifraudEvaluationResult(
    bool IsDuplicate,
    string Fingerprint,
    string VoucherHash,
    string? ConflictingPaymentId,
    string? ConflictingOrderCode,
    string? Message
);
