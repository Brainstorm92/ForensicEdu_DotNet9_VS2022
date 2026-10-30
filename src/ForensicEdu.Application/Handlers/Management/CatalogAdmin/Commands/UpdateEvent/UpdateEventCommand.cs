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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateEvent;

public sealed record UpdateEventCommand(string Id, EventInput Input) : IRequest<ApplicationResult>;
public sealed class UpdateEventCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateEventCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateEventCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=await db.LiveEvents.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.Title=input.Title.Trim();item.Type=input.Type;item.Description=input.Description;item.Price=input.Price;item.EventDate=input.EventDate;
        item.TimeSlot=input.TimeSlot;item.Speaker=input.Speaker;item.SpeakerBio=input.SpeakerBio;item.RoomUrl=input.RoomUrl;item.BannerUrl=input.BannerUrl;item.AccessStatus=input.AccessStatus;
        EndpointSupport.Audit(db,current,"EVENT_SAVED","LiveEvent",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new EventInputValidator());
    }
}
