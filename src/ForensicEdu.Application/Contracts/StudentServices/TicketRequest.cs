using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.StudentServices;

public record TicketRequest(string Subject, string Category, string Message);
