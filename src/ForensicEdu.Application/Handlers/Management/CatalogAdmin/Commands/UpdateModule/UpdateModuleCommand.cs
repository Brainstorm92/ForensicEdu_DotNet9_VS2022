using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateModule;

public sealed record UpdateModuleCommand(string Id, string ModuleId, ModuleInput Input) : IRequest<ApplicationResult>;
