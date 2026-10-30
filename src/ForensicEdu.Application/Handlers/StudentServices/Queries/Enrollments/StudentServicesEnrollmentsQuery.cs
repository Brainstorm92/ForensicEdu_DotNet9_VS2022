using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.Enrollments;

public sealed record StudentServicesEnrollmentsQuery() : IRequest<ApplicationResult>;
