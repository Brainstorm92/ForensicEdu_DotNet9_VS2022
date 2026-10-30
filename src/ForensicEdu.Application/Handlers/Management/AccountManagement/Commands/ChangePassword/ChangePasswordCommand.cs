using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Application.Common.Management;
using ForensicEdu.Application.Common.Validation;
using ForensicEdu.Application.Contracts.Management;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.ChangePassword;

public sealed record ChangePasswordCommand(PasswordInput Input) : IRequest<ApplicationResult>;
public sealed class ChangePasswordCommandHandler(IApplicationDbContext db, ICurrentUser current, IPasswordService passwords) : IRequestHandler<ChangePasswordCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var item=await db.Users.FirstOrDefaultAsync(x=>x.Id==current.UserId,ct);if(item==null)return ApplicationResult.NotFound();
        if(!passwords.Verify(request.Input.CurrentPassword,item.PasswordHash))return ApplicationResult.Invalid(new {message="La contraseña actual es incorrecta."});
        item.PasswordHash=passwords.Hash(request.Input.NewPassword);EndpointSupport.Audit(db,current,"PASSWORD_CHANGED","User",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new PasswordInputValidator());
    }
}
