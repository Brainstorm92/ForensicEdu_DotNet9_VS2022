using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListSupportTickets;

public sealed class ListSupportTicketsQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListSupportTicketsQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListSupportTicketsQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var query=db.SupportTickets.AsNoTracking().AsQueryable();if(request.Status.HasValue)query=query.Where(x=>x.Status==request.Status.Value);
        if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Subject.ToLower().Contains(request.Search.Trim().ToLower()) || x.TicketCode.Contains(request.Search));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.TicketCode,x.UserId,x.UserName,x.UserEmail,x.Subject,x.Category,x.Message,x.Status,x.CreatedAt,x.ResponsesJson}),request.Page,request.PageSize,ct));
    }
}
