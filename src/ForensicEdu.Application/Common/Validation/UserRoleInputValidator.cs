using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class UserRoleInputValidator : AbstractValidator<UserRoleInput>
{
    public UserRoleInputValidator() { RuleFor(x=>x.Role).IsInEnum(); }
}
