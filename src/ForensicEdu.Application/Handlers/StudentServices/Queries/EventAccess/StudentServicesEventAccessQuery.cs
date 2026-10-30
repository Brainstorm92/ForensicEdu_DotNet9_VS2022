using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Queries.EventAccess;

public sealed record StudentServicesEventAccessQuery(string Id) : IRequest<ApplicationResult>;
