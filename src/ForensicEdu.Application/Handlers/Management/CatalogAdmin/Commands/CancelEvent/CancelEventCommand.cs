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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CancelEvent;

public sealed record CancelEventCommand(string Id) : IRequest<ApplicationResult>;
public sealed class CancelEventCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CancelEventCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CancelEventCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var item=await db.LiveEvents.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.AccessStatus="CANCELADO";EndpointSupport.Audit(db,current,"EVENT_REMOVED","LiveEvent",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
