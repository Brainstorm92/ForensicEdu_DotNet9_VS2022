using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.StudentServices;

namespace ForensicEdu.Application.Handlers.StudentServices.Commands.CreateTicket;
public sealed record StudentServicesCreateTicketCommand(TicketRequest Request) : IRequest<ApplicationResult>;
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
        return ApplicationResult.Success(ticket);
    }
}

public sealed class StudentServicesCreateTicketCommandValidator : AbstractValidator<StudentServicesCreateTicketCommand>
{
    public StudentServicesCreateTicketCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Message).NotEmpty().MaximumLength(10000);
        });
    }
}
