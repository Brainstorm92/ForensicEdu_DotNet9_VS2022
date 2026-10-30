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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetPaymentVoucher;

public sealed record GetPaymentVoucherQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetPaymentVoucherQueryHandler(IApplicationDbContext db, ICurrentUser current, IVoucherStorage storage) : IRequestHandler<GetPaymentVoucherQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPaymentVoucherQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var item=await db.PaymentEvidences.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        var file=await storage.ReadAsync(item.VoucherFileUrl,ct);if(file==null)return ApplicationResult.NotFound(new {message="Comprobante no disponible."});
        return ApplicationResult.Download(file.Bytes,file.ContentType,file.FileName);
    }
}

public sealed class GetPaymentVoucherQueryValidator : AbstractValidator<GetPaymentVoucherQuery>
{
    public GetPaymentVoucherQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
