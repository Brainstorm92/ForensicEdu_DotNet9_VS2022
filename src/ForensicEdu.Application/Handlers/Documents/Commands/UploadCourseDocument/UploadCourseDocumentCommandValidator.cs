using FluentValidation;
using ForensicEdu.Application.Common.Management;
namespace ForensicEdu.Application.Handlers.Documents.Commands.UploadCourseDocument;
public sealed class UploadCourseDocumentCommandValidator:AbstractValidator<UploadCourseDocumentCommand>
{
 public UploadCourseDocumentCommandValidator(){
  RuleFor(x=>x.CourseId).NotEmpty().MaximumLength(200);RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
  RuleFor(x=>x.Title).NotEmpty().MaximumLength(300);RuleFor(x=>x.FileName).NotEmpty().MaximumLength(250);
  RuleFor(x=>x.Bytes).NotEmpty().Must(x=>x!=null && x.Length<=PrivateDocuments.MaxBytes).WithMessage("El material debe pesar como máximo 20 MB.");
 }
}
