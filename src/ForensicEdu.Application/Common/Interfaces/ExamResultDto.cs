using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

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
