using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record PasswordInput(string CurrentPassword, string NewPassword);
