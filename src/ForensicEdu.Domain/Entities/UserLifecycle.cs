using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Domain.Entities;
public partial class User
{
    public void RenewFromApprovedPayment(DateTime approvedAt)
    {
        ValidUntil = approvedAt.AddYears(2);
        Status = AccountStatus.ACTIVO;
        AccountStatus = AccountStatus.ACTIVO;
        LastPurchaseDate = approvedAt;
        HasApprovedPurchase = true;
    }
}
