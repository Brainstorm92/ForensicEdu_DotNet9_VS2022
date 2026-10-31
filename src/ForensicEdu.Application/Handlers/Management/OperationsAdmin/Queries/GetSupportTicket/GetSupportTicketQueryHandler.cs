using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSupportTicket;

public sealed class GetSupportTicketQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetSupportTicketQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetSupportTicketQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var item=await db.SupportTickets.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new {x.Id,x.TicketCode,x.UserId,x.UserName,x.UserEmail,x.Subject,x.Category,x.Message,x.Status,x.CreatedAt,x.ResponsesJson}).FirstOrDefaultAsync(ct);return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}
