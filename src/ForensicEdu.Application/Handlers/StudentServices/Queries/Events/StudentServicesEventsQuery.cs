using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Events;

public sealed record StudentServicesEventsQuery() : IRequest<ApplicationResult>;
