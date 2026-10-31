using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetTicketStatus;

public sealed record SetTicketStatusCommand(string Id, TicketStatusInput Input) : IRequest<ApplicationResult>;
