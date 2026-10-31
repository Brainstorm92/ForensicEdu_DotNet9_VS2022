using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Orders;
public record CreateOrderRequest(
        string UserId,
        string ProductId,
        ProductType ProductType,
        decimal Amount,
        string BankName,
        string OperationNumber,
        DateTime OperationDate,
        string AccountHolderName
    );
