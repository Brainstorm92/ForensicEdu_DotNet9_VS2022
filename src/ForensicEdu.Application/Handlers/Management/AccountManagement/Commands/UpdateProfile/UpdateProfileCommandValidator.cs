using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new ProfileInputValidator());
    }
}
