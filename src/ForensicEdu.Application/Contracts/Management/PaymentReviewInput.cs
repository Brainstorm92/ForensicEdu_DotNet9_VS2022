using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record PaymentReviewInput(PaymentStatus Status, string Reason);
