using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record ModuleInput(string Title, int OrderIndex, string Duration = "", string VideoStreamingUrl = "");
