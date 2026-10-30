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
namespace ForensicEdu.Application.Handlers.Management.PublicCatalog.Queries.GetPublicEvent;

public sealed record GetPublicEventQuery(string Id) : IRequest<ApplicationResult>;
public sealed class GetPublicEventQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPublicEventQuery,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(GetPublicEventQuery request, CancellationToken ct)
    {
        var item=await db.LiveEvents.AsNoTracking().Where(x=>x.Id==request.Id).Select(x=>new { x.Id,x.Code,x.Title,x.Type,x.Description,x.Price,x.EventDate,x.TimeSlot,x.Speaker,x.SpeakerBio,x.AccessStatus,x.BannerUrl }).FirstOrDefaultAsync(ct);
        return item == null ? ApplicationResult.NotFound() : ApplicationResult.Success(item);
    }
}

public sealed class GetPublicEventQueryValidator : AbstractValidator<GetPublicEventQuery>
{
    public GetPublicEventQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
