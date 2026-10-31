using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetDocumentAccess;

public sealed class GetDocumentAccessQueryValidator : AbstractValidator<GetDocumentAccessQuery>
{
    public GetDocumentAccessQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.DocumentId).NotEmpty().MaximumLength(200);
    }
}
