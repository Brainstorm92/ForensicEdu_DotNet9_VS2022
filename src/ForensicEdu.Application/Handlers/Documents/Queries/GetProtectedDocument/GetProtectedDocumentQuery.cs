using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Documents.Queries.GetProtectedDocument;
public sealed record GetProtectedDocumentQuery(string EnrollmentId,string DocumentId):IRequest<ApplicationResult>;
