using FluentValidation;
using ForensicEdu.Application.Contracts.Management;
namespace ForensicEdu.Application.Common.Validation;

public sealed class UserStatusInputValidator : AbstractValidator<UserStatusInput>
{
    public UserStatusInputValidator() { RuleFor(x=>x.Status).Must(s=>s is ForensicEdu.Domain.Enums.AccountStatus.SUSPENDIDO or ForensicEdu.Domain.Enums.AccountStatus.ACTIVO); }
}
