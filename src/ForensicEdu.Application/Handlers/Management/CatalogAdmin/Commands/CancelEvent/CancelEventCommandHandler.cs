using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CancelEvent;

public sealed class CancelEventCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CancelEventCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CancelEventCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.LiveEvents.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.AccessStatus="CANCELADO";EndpointSupport.Audit(db,current,"EVENT_REMOVED","LiveEvent",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}
