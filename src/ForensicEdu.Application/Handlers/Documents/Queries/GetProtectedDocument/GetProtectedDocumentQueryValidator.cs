using FluentValidation;
namespace ForensicEdu.Application.Handlers.Documents.Queries.GetProtectedDocument;
public sealed class GetProtectedDocumentQueryValidator:AbstractValidator<GetProtectedDocumentQuery>
{public GetProtectedDocumentQueryValidator(){RuleFor(x=>x.EnrollmentId).NotEmpty().MaximumLength(200);RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);}}
