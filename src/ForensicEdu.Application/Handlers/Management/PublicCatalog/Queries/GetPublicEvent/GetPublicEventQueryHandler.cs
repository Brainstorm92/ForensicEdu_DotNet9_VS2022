using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicEvent;

public sealed class GetPublicEventQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPublicEventQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPublicEventQuery request, CancellationToken ct)
    {
        var item=await db.LiveEvents.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new { x.Id,x.Code,x.Title,x.Type,x.Description,x.Price,x.EventDate,x.TimeSlot,x.Speaker,x.SpeakerBio,x.AccessStatus,x.BannerUrl }).FirstOrDefaultAsync(ct);
        return item == null ? ApplicationResult.NotFound() : ApplicationResult.Success(item);
    }
}
