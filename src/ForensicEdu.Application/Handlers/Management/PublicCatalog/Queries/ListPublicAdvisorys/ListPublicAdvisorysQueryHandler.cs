using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicAdvisorys;

public sealed class ListPublicAdvisorysQueryHandler(IApplicationDbContext db) : IRequestHandler<ListPublicAdvisorysQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListPublicAdvisorysQuery request, CancellationToken ct)
    {
        var query=db.AdvisoryServices.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(request.Search)) query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Title).Select(x=>new { x.Id,x.Title,x.Specialist,x.Specialty,x.Description,x.Price }),request.Page,request.PageSize,ct));
    }
}
