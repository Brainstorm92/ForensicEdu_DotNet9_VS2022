using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.DeleteDocument;

public sealed record DeleteDocumentCommand(string Id, string ModuleId, string DocumentId) : IRequest<ApplicationResult>;
