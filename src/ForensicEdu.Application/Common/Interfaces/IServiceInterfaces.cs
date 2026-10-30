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

public interface IQrCodeService
{
    byte[] GeneratePngQrCode(string payloadUrl, int pixelsPerModule = 20);
    string GenerateBase64QrCode(string payloadUrl, int pixelsPerModule = 20);
}

public interface IPdfCertificateService
{
    Task<byte[]> GenerateCertificatePdfAsync(
        string studentFullName,
        string studentDni,
        string courseTitle,
        int academicHours,
        string certificateCode,
        string qrValidationUrl,
        DateTime issueDate,
        decimal finalGrade,
        CancellationToken cancellationToken = default
    );
}

public record ExamSubmissionDto(
    string EnrollmentId,
    string UserId,
    Dictionary<string, int> SelectedAnswers, // QuestionId -> SelectedOptionIndex
    int DurationSeconds
);

public record ExamResultDto(
    string AttemptId,
    int AttemptNumber,
    decimal Score,
    bool Approved,
    string Status, // APROBADO, DESAPROBADO_TEMPORAL, DESAPROBADO_DEFINITIVO
    int CorrectCount,
    int TotalQuestions,
    DateTime? NextAttemptAllowedAt,
    string? CertificateCode,
    string Message
);

public interface IEvaluationService
{
    Task<ExamResultDto> SubmitExamAttemptAsync(ExamSubmissionDto submission, CancellationToken cancellationToken = default);
}

public interface IAccountLifecycleService
{
    Task ProcessDailyAccountExpirationsAsync(CancellationToken cancellationToken = default);
    Task ExtendValidityOnApprovedPaymentAsync(string userId, DateTime paymentDate, CancellationToken cancellationToken = default);
}
