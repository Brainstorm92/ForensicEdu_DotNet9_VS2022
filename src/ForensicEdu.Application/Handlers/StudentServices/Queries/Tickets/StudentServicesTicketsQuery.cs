using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Tickets;

public sealed record StudentServicesTicketsQuery() : IRequest<ApplicationResult>;
