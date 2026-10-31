using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class CertificateVerificationLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CertificateCode { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsValid { get; set; } = true;
}
