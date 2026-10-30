using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.StudentServices;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.CreateTicket;

public sealed record StudentServicesCreateTicketCommand(TicketRequest Request) : IRequest<ApplicationResult>;
