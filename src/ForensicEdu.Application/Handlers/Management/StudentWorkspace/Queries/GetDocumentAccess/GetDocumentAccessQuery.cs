using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;

public sealed record GetDocumentAccessQuery(string Id, string DocumentId) : IRequest<ApplicationResult>;
