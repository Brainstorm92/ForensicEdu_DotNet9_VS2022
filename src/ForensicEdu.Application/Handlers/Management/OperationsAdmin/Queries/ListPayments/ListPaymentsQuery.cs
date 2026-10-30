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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListPayments;

public sealed record ListPaymentsQuery(PaymentStatus? Status = null, int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListPaymentsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListPaymentsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListPaymentsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var query=db.PaymentEvidences.AsNoTracking().AsQueryable();if(request.Status.HasValue)query=query.Where(x=>x.Status==request.Status.Value);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Order!.OrderCode.Contains(request.Search) || x.OperationNumber.Contains(request.Search));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.RegisteredAt).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.OrderId,orderCode=x.Order!.OrderCode,userId=x.Order.UserId,x.BankName,x.OperationNumber,x.OperationDate,x.Amount,x.AccountHolderName,x.Status,x.RegisteredAt,x.ValidatedAt,x.ReviewedAt,hasVoucher=x.VoucherFileUrl!=""}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListPaymentsQueryValidator : AbstractValidator<ListPaymentsQuery>
{
    public ListPaymentsQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
        RuleFor(x=>x.Status).IsInEnum().When(x=>x.Status.HasValue);
    }
}
