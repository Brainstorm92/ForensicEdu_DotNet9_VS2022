using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.GetSupportTicket;

public sealed record GetSupportTicketQuery(string Id) : IRequest<ApplicationResult>;
