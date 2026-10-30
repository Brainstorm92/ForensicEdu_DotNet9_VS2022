using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public class ExamAttempt
{
    public string QuestionsSnapshotJson { get; set; } = "[]";
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnrollmentId { get; set; } = string.Empty;
    public Enrollment? Enrollment { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; } = 1;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; } = 20.0m;
    public decimal PassingScore { get; set; } = 13.0m;
    public bool Approved { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.NO_INICIADA;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? NextAttemptAllowedAt { get; set; }
    public int DurationSeconds { get; set; } = 600;
    public string AnswersJson { get; set; } = "{}";
}
