using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListUsers;

public sealed class ListUsersQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListUsersQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListUsersQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var query=db.Users.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Email.ToLower().Contains(request.Search.Trim().ToLower()) || x.FullName.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.FullName).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.Nombres,x.Apellidos,x.FullName,x.Email,x.Dni,x.Phone,x.Role,x.Status,x.ValidUntil,x.HasApprovedPurchase,x.RegisteredAt}),request.Page,request.PageSize,ct));
    }
}
