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
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateEvent;

public sealed record CreateEventCommand(EventInput Input) : IRequest<ApplicationResult>;
public sealed class CreateEventCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<CreateEventCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(CreateEventCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "2")) return ApplicationResult.Forbidden();
        var input=request.Input;
        var item=new LiveEvent();db.LiveEvents.Add(item);
        item.Title=input.Title.Trim();item.Type=input.Type;item.Description=input.Description;item.Price=input.Price;item.EventDate=input.EventDate;
        item.TimeSlot=input.TimeSlot;item.Speaker=input.Speaker;item.SpeakerBio=input.SpeakerBio;item.RoomUrl=input.RoomUrl;item.BannerUrl=input.BannerUrl;item.AccessStatus=input.AccessStatus;
        EndpointSupport.Audit(db,current,"EVENT_SAVED","LiveEvent",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.Success(new {item.Id,item.Title});
    }
}

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new EventInputValidator());
    }
}
