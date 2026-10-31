using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Events;

public sealed class StudentServicesEventsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesEventsQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesEventsQuery message, CancellationToken cancellationToken)
    {


        var products = db.Orders.Where(o => o.UserId == current.UserId && o.Status == OrderStatus.APROBADO && o.ProductType == ProductType.EVENTO).Select(o => o.ProductId);
        return ApplicationResult.Success(await db.LiveEvents.Where(e => products.Contains(e.Id))
            .Select(e => new { e.Id, e.Title, e.EventDate, e.TimeSlot, e.AccessStatus }).ToListAsync(cancellationToken));
    }
}
