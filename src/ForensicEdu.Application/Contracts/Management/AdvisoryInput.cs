using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record AdvisoryInput(string Title, string Specialist, string Specialty, string Description, decimal Price, string WhatsappContact);
