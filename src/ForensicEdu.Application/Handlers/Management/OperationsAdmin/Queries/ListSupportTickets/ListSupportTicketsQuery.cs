using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Queries.ListSupportTickets;

public sealed record ListSupportTicketsQuery(TicketStatus? Status = null, int Page = 1, int PageSize = 20, string? Search = null) : IRequest<ApplicationResult>;
