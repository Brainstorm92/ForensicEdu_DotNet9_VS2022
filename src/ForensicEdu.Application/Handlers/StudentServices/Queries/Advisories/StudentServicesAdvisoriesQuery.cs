using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Advisories;

public sealed record StudentServicesAdvisoriesQuery() : IRequest<ApplicationResult>;
