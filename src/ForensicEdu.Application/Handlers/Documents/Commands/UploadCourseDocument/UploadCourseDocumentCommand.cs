using MediatR;
using ForensicEdu.Application.Common.Results;
namespace ForensicEdu.Application.Handlers.Documents.Commands.UploadCourseDocument;
public sealed record UploadCourseDocumentCommand(string CourseId,string ModuleId,string Title,string FileName,byte[] Bytes):IRequest<ApplicationResult>;
