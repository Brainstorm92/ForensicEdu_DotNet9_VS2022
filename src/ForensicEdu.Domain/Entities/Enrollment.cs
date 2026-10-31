using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class Enrollment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public string CourseId { get; set; } = string.Empty;
    public Course? Course { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; } = 0;
    public string CompletedModulesJson { get; set; } = "[]";
    public string Status { get; set; } = "ACTIVA";
    public ExamStatus ExamStatus { get; set; } = ExamStatus.NO_INICIADA;
    public decimal? FinalGrade { get; set; }
    public decimal? FirstAttemptScore { get; set; }
    public decimal? SecondAttemptScore { get; set; }
    public int AttemptsUsed { get; set; } = 0;
    public DateTime? SecondAttemptAvailableAt { get; set; }
    public string? CertificateId { get; set; }

    public DigitalCertificate? DigitalCertificate { get; set; }
    public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
