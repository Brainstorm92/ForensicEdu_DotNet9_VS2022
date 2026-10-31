using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedEvent;

public sealed class GetManagedEventQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetManagedEventQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetManagedEventQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.LiveEvents.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==request.Id,ct);return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}
