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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetTicketStatus;

public sealed record SetTicketStatusCommand(string Id, TicketStatusInput Input) : IRequest<ApplicationResult>;
public sealed class SetTicketStatusCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SetTicketStatusCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SetTicketStatusCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1", "5")) return ApplicationResult.Forbidden();
        var item=await db.SupportTickets.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        item.Status=request.Input.Status;EndpointSupport.Audit(db,current,"TICKET_STATUS_CHANGED","SupportTicket",item.Id,item.Status.ToString());await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class SetTicketStatusCommandValidator : AbstractValidator<SetTicketStatusCommand>
{
    public SetTicketStatusCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new TicketStatusInputValidator());
    }
}
