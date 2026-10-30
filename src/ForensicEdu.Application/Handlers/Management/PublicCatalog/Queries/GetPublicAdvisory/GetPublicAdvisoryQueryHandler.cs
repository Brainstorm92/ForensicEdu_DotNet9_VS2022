using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicAdvisory;

public sealed class GetPublicAdvisoryQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPublicAdvisoryQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPublicAdvisoryQuery request, CancellationToken ct)
    {
        var item=await db.AdvisoryServices.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new { x.Id,x.Title,x.Specialist,x.Specialty,x.Description,x.Price }).FirstOrDefaultAsync(ct);
        return item == null ? ApplicationResult.NotFound() : ApplicationResult.Success(item);
    }
}
