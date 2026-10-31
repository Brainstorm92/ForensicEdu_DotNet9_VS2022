using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.ListPublicEvents;

public sealed class ListPublicEventsQueryHandler(IApplicationDbContext db) : IRequestHandler<ListPublicEventsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListPublicEventsQuery request, CancellationToken ct)
    {
        var query=db.LiveEvents.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(request.Search)) query=query.Where(x=>x.Title.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.Title).Select(x=>new { x.Id,x.Code,x.Title,x.Type,x.Description,x.Price,x.EventDate,x.TimeSlot,x.Speaker,x.SpeakerBio,x.AccessStatus,x.BannerUrl }),request.Page,request.PageSize,ct));
    }
}
