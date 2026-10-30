using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListUsers;

public sealed record ListUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
public sealed class ListUsersQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<ListUsersQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ListUsersQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var query=db.Users.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(request.Search))query=query.Where(x=>x.Email.ToLower().Contains(request.Search.Trim().ToLower()) || x.FullName.ToLower().Contains(request.Search.Trim().ToLower()));
        return ApplicationResult.Success(await EndpointSupport.PageAsync(query.OrderBy(x=>x.FullName).ThenBy(x=>x.Id).Select(x=>new {x.Id,x.Nombres,x.Apellidos,x.FullName,x.Email,x.Dni,x.Phone,x.Role,x.Status,x.ValidUntil,x.HasApprovedPurchase,x.RegisteredAt}),request.Page,request.PageSize,ct));
    }
}

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,1000000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,100);
        RuleFor(x=>x.Search).MaximumLength(200);
    }
}
