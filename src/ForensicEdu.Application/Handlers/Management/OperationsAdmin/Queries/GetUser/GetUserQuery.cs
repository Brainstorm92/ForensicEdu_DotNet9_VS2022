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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetUser;

public sealed record GetUserQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetUserQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetUserQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetUserQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var item=await db.Users.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new {x.Id,x.Nombres,x.Apellidos,x.FullName,x.Email,x.Dni,x.Phone,x.Role,x.Status,x.ValidUntil,x.HasApprovedPurchase,x.RegisteredAt}).FirstOrDefaultAsync(ct);return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
