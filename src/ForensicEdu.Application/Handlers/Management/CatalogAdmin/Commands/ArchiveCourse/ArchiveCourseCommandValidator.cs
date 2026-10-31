using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.CatalogAdmin.Commands.ArchiveCourse;

public sealed class ArchiveCourseCommandValidator : AbstractValidator<ArchiveCourseCommand>
{
    public ArchiveCourseCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
    }
}
