using System.Linq.Expressions;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Domain.Policies;

public static class EnrollmentPolicy
{
    // Actual completed attempts are authoritative; counters alone must not unlock a purchase.
    public static readonly Expression<Func<Enrollment,bool>> Exhausted = e =>
        (e.Status == "ACTIVA" || e.Status == "DESAPROBADA") &&
        e.ExamStatus != ExamStatus.APROBADO && e.DigitalCertificate == null &&
        e.ExamAttempts.Count(a => a.CompletedAt != null) >= AcademicPolicy.MaximumAttempts &&
        !e.ExamAttempts.Any(a => a.Approved || a.CompletedAt == null);
}
