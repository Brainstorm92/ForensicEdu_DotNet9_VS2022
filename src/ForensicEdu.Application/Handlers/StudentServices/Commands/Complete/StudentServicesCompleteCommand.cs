using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.StudentServices.Commands.Complete;

public sealed record StudentServicesCompleteCommand(string Id, string ModuleId) : IRequest<ApplicationResult>;
