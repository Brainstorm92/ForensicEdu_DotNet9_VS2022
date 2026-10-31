using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetTicketStatus;

public sealed class SetTicketStatusCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SetTicketStatusCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SetTicketStatusCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var item=await db.SupportTickets.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.Status=request.Input.Status;EndpointSupport.Audit(db,current,"TICKET_STATUS_CHANGED","SupportTicket",item.Id,item.Status.ToString());await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}
