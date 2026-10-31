using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateModule;

public sealed record CreateModuleCommand(string Id, ModuleInput Input) : IRequest<ApplicationResult>;
