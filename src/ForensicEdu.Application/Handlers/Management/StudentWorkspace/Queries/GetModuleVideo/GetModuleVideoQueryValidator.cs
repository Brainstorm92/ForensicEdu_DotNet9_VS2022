using FluentValidation;
namespace ForensicEdu.Application.Handlers.Management.StudentWorkspace.Queries.GetModuleVideo;

public sealed class GetModuleVideoQueryValidator : AbstractValidator<GetModuleVideoQuery>
{
    public GetModuleVideoQueryValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.ModuleId).NotEmpty().MaximumLength(200);
    }
}
