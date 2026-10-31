using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.StudentServices;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Reply;

public sealed record StudentServicesReplyCommand(string Id, TicketReply Request) : IRequest<ApplicationResult>;
