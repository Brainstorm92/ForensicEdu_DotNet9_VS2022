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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedEvent;

public sealed record GetManagedEventQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetManagedEventQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetManagedEventQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetManagedEventQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.LiveEvents.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==request.Id,ct);return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}

public sealed class GetManagedEventQueryValidator : AbstractValidator<GetManagedEventQuery>
{
    public GetManagedEventQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
