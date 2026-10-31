using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForensicEdu.Application.Common.Interfaces;

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
