using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPayment;

public sealed record GetPaymentQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetPaymentQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetPaymentQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPaymentQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var item=await db.PaymentEvidences.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new {x.Id,x.OrderId,orderCode=x.Order!.OrderCode,userId=x.Order.UserId,x.BankName,x.OperationNumber,x.OperationDate,x.Amount,x.AccountHolderName,x.Status,x.RegisteredAt,x.ValidatedAt,x.ReviewedAt,hasVoucher=x.VoucherFileUrl!=""}).FirstOrDefaultAsync(ct);
        return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}

public sealed class GetPaymentQueryValidator : AbstractValidator<GetPaymentQuery>
{
    public GetPaymentQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
