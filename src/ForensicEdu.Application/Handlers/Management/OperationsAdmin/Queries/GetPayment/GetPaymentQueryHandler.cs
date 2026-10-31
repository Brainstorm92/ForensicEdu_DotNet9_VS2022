using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPayment;

public sealed class GetPaymentQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetPaymentQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var item=await db.PaymentEvidences.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new {x.Id,x.OrderId,orderCode=x.Order!.OrderCode,userId=x.Order.UserId,productId=x.Order.ProductId,productType=x.Order.ProductType,productTitle=x.Order.ProductTitle,userName=x.Order.User!.FullName,userEmail=x.Order.User.Email,x.BankName,x.OperationNumber,x.OperationDate,x.Amount,x.AccountHolderName,x.Status,x.RegisteredAt,x.ValidatedAt,x.ReviewedAt,hasVoucher=x.VoucherFileUrl!=""}).FirstOrDefaultAsync(ct);
        return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}
