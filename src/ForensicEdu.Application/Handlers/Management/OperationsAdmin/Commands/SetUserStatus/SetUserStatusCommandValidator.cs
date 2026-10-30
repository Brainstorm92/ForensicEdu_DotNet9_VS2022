using FluentValidation;
using ForensicEdu.Application.Common.Validation;
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserStatus;

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new UserStatusInputValidator());
    }
}
