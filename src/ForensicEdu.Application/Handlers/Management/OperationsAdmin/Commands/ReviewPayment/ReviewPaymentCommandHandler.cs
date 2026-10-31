using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.ReviewPayment;

public sealed class ReviewPaymentCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ReviewPaymentCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ReviewPaymentCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "3")) return ApplicationResult.Forbidden();
        var item=await db.PaymentEvidences.Include(x=>x.Order).FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        if(item.Status is PaymentStatus.APROBADO or PaymentStatus.DUPLICADO)return ApplicationResult.Conflict(new {message="Un pago aprobado o duplicado no admite esta transición."});
        item.Status=request.Input.Status;item.ReviewedAt=DateTime.UtcNow;item.ReviewedByUserId=current.UserId;
        EndpointSupport.Audit(db,current,"PAYMENT_REVIEWED","PaymentEvidence",item.Id,$"{item.Status}: {request.Input.Reason}");await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}
