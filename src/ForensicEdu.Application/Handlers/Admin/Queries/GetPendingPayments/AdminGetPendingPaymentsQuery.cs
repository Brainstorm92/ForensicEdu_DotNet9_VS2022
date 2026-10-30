using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Admin;

namespace ForensicEdu.Application.Handlers.Admin.Queries.GetPendingPayments;
public sealed record AdminGetPendingPaymentsQuery() : IRequest<ApplicationResult>;
public sealed class AdminGetPendingPaymentsQueryHandler(IApplicationDbContext _context) : IRequestHandler<AdminGetPendingPaymentsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AdminGetPendingPaymentsQuery message, CancellationToken cancellationToken)
    {


        var pending = await _context.PaymentEvidences
            .Include(p => p.Order)
                .ThenInclude(o => o!.User)
            .Where(p => p.Status == PaymentStatus.PENDIENTE)
            .OrderByDescending(p => p.RegisteredAt)
            .Select(p=>new {p.Id,p.OrderId,orderCode=p.Order!.OrderCode,userId=p.Order.UserId,userName=p.Order.User!.FullName,p.BankName,p.OperationNumber,p.OperationDate,p.Amount,p.AccountHolderName,p.Status,p.RegisteredAt,hasVoucher=p.VoucherFileUrl!=""})
            .ToListAsync(cancellationToken);

        return ApplicationResult.Success(pending);
    }
}
