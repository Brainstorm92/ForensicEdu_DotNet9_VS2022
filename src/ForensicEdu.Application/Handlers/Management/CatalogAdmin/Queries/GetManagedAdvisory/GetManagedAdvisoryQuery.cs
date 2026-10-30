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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Queries.GetManagedAdvisory;

public sealed record GetManagedAdvisoryQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetManagedAdvisoryQueryHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<GetManagedAdvisoryQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetManagedAdvisoryQuery request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.AdvisoryServices.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==request.Id,ct);return item==null?ApplicationResult.NotFound():ApplicationResult.Success(item);
    }
}

public sealed class GetManagedAdvisoryQueryValidator : AbstractValidator<GetManagedAdvisoryQuery>
{
    public GetManagedAdvisoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
