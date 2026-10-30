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
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicAdvisory;

public sealed record GetPublicAdvisoryQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetPublicAdvisoryQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPublicAdvisoryQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPublicAdvisoryQuery request, CancellationToken ct)
    {
        var item=await db.AdvisoryServices.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new { x.Id,x.Title,x.Specialist,x.Specialty,x.Description,x.Price }).FirstOrDefaultAsync(ct);
        return item == null ? ApplicationResult.NotFound() : ApplicationResult.Success(item);
    }
}

public sealed class GetPublicAdvisoryQueryValidator : AbstractValidator<GetPublicAdvisoryQuery>
{
    public GetPublicAdvisoryQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
