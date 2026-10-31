namespace ForensicEdu.Domain.Policies;
public static class AcademicPolicy
{
    public const decimal MaximumScore = 20m;
    public const decimal PassingScore = 13m;
    public const int MaximumAttempts = 2;
    public const int ExamDurationSeconds = 600;
    public static bool IsApproved(decimal score) => score >= PassingScore;
}
