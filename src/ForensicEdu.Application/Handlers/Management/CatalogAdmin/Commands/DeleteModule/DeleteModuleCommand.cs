using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteModule;

public sealed record DeleteModuleCommand(string Id, string ModuleId) : IRequest<ApplicationResult>;
