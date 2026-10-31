using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedEvents;

public sealed class ListManagedEventsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListManagedEventsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListManagedEventsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.LiveEvents.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Title).ThenBy(x=>x.Id),request.Page,request.PageSize,ct));
    }
}
