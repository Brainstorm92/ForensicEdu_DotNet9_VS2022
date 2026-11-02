namespace ForensicEdu.Application.Common.Validation;
public static class BirthDateRules
{
 public static bool Valid(DateOnly? value)=>value.HasValue&&value.Value!=default&&value.Value<=DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-5));
}
