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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserRole;

public sealed record SetUserRoleCommand(string Id, UserRoleInput Input) : IRequest<ApplicationResult>;
public sealed class SetUserRoleCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SetUserRoleCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SetUserRoleCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1")) return ApplicationResult.Forbidden();
        if(request.Id==current.UserId)return ApplicationResult.Conflict(new {message="No puede cambiar su propio rol."});
        var item=await db.Users.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        if(item.Role==UserRole.SUPERADMIN)return ApplicationResult.Conflict(new {message="No puede degradar a otro superadministrador desde este endpoint."});
        item.Role=request.Input.Role;EndpointSupport.Audit(db,current,"USER_ROLE_CHANGED","User",item.Id,item.Role.ToString());await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class SetUserRoleCommandValidator : AbstractValidator<SetUserRoleCommand>
{
    public SetUserRoleCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new UserRoleInputValidator());
    }
}
