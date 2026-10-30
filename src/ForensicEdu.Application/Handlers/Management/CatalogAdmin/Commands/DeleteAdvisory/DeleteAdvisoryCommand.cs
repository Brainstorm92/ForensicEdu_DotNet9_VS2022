using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteAdvisory;

public sealed record DeleteAdvisoryCommand(string Id) : IRequest<ApplicationResult>;
