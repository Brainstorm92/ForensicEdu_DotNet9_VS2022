using MediatR;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.UpdateDocument;

public sealed record UpdateDocumentCommand(string Id, string ModuleId, string DocumentId, DocumentInput Input) : IRequest<ApplicationResult>;
