using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record QuestionInput(string Id, string Question, string[] Options, int CorrectOptionIndex, string Explanation = "");
