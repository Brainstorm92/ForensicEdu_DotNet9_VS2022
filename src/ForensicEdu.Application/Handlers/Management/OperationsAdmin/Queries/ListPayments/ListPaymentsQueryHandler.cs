using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListPayments;

public sealed class ListPaymentsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListPaymentsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var query=db.PaymentEvidences.AsNoTracking().AsQueryable();if(request.Status.HasValue)query=query.Where(x=>x.Status==request.Status.Value);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Order!.OrderCode.Contains(request.Search) || x.OperationNumber.Contains(request.Search));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.RegisteredAt).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.OrderId,orderCode=x.Order!.OrderCode,userId=x.Order.UserId,productId=x.Order.ProductId,productType=x.Order.ProductType,productTitle=x.Order.ProductTitle,userName=x.Order.User!.FullName,userEmail=x.Order.User.Email,x.BankName,x.OperationNumber,x.OperationDate,x.Amount,x.AccountHolderName,x.Status,x.RegisteredAt,x.ValidatedAt,x.ReviewedAt,hasVoucher=x.VoucherFileUrl!=""}),request.Page,request.PageSize,ct));
    }
}
