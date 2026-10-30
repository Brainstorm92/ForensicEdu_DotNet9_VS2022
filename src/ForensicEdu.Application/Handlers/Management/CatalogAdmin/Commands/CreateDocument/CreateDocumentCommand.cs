using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.CreateDocument;

public sealed record CreateDocumentCommand(string Id, string ModuleId, DocumentInput Input) : IRequest<ApplicationResult>;
