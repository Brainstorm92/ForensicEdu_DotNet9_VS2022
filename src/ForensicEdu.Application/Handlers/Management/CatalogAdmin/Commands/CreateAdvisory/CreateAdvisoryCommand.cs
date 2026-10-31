using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateAdvisory;

public sealed record CreateAdvisoryCommand(AdvisoryInput Input) : IRequest<ApplicationResult>;
