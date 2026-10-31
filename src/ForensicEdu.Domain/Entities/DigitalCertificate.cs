using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class DigitalCertificate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnrollmentId { get; set; } = string.Empty;
    public Enrollment? Enrollment { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string CertificateCode { get; set; } = string.Empty;
    public string QrValidationUrl { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentDni { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int AcademicHours { get; set; } = 40;
    public decimal FinalGrade { get; set; }
    public CertificateStatus Status { get; set; } = CertificateStatus.VALIDO;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
}
