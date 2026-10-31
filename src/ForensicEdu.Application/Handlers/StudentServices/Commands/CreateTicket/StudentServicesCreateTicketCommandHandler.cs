using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.CreateTicket;

public sealed class StudentServicesCreateTicketCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<StudentServicesCreateTicketCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(StudentServicesCreateTicketCommand message, CancellationToken cancellationToken)
    {
        var request = message.Request;

        if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 10000) return ApplicationResult.Invalid();
        var user = await db.Users.FirstAsync(u => u.Id == current.UserId);
        var ticket = new SupportTicket { UserId = current.UserId, UserName = user.FullName, UserEmail = user.Email,
            Subject = request.Subject.Trim(), Category = request.Category, Message = request.Message.Trim(),
            TicketCode = $"TK-{DateTime.UtcNow.Year}-{Guid.NewGuid():N}" };
        db.SupportTickets.Add(ticket);
        db.AuditLogs.Add(new AuditLog { UserId = current.UserId, Action = "TICKET_CREATED", EntityType = "SupportTicket", EntityId = ticket.Id });
        await db.SaveChangesAsync(cancellationToken);
        return ApplicationResult.Success(new {ticket.Id,ticket.TicketCode,ticket.UserId,ticket.UserName,ticket.UserEmail,ticket.Subject,ticket.Category,ticket.Message,ticket.Status,ticket.CreatedAt,ticket.ResponsesJson});
    }
}
