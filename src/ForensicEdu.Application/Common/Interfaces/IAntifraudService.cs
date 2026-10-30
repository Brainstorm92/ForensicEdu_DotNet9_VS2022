using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

public interface IAntifraudService
{
    string GenerateFingerprint(string bank, string operationNumber, DateTime date, decimal amount, string accountHolder);
    string ComputeVoucherSha256(byte[] voucherBytes);
    string ComputeVoucherSha256(Stream voucherStream);
    Task<AntifraudEvaluationResult> EvaluatePaymentAsync(
        string bank,
        string operationNumber,
        DateTime date,
        decimal amount,
        string accountHolder,
        byte[]? voucherBytes = null,
        CancellationToken cancellationToken = default
    );
}
