using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateAdvisory;

public sealed record UpdateAdvisoryCommand(string Id, AdvisoryInput Input) : IRequest<ApplicationResult>;
