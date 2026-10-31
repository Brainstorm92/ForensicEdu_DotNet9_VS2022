using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.ListManagedAdvisorys;

public sealed class ListManagedAdvisorysQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListManagedAdvisorysQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListManagedAdvisorysQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var query=db.AdvisoryServices.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Title).ThenBy(x=>x.Id),request.Page,request.PageSize,ct));
    }
}
