using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Reply;
public sealed record StudentServicesReplyCommand(string Id, TicketReply Request) : IRequest<ApplicationResult>;
public sealed class StudentServicesReplyCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesReplyCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesReplyCommand message, CancellationToken cancellationToken)
    {
        var id = message.Id;
        var request = message.Request;

        var ticket = await db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return ApplicationResult.NotFound();
        var staff = current.IsInRole("1") || current.IsInRole("5");
        if (ticket.UserId != current.UserId && !staff) return ApplicationResult.Forbidden();
        if (ticket.Status == TicketStatus.CERRADO || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 10000) return ApplicationResult.Invalid();
        var responses = JsonSerializer.Deserialize<List<JsonElement>>(ticket.ResponsesJson) ?? new();
        responses.Add(JsonSerializer.SerializeToElement(new { authorId = current.UserId, message = request.Message.Trim(), timestamp = DateTime.UtcNow }));
        ticket.ResponsesJson = JsonSerializer.Serialize(responses);
        ticket.Status = staff ? TicketStatus.RESPONDIDO : TicketStatus.ABIERTO;
        db.AuditLogs.Add(new AuditLog { UserId = current.UserId, Action = "TICKET_REPLIED", EntityType = "SupportTicket", EntityId = ticket.Id });
        await db.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success(ticket);
    }
}

public sealed class StudentServicesReplyCommandValidator : AbstractValidator<StudentServicesReplyCommand>
{
    public StudentServicesReplyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Message).NotEmpty().MaximumLength(10000);
        });
    }
}
