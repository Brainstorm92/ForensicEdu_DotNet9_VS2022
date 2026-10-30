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
namespace ForensicEdu.Application.Handlers.Management.OperationsAdmin.Commands.SetUserStatus;

public sealed record SetUserStatusCommand(string Id, UserStatusInput Input) : IRequest<ApplicationResult>;
public sealed class SetUserStatusCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<SetUserStatusCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(SetUserStatusCommand request, CancellationToken ct)
    {
        if (!EndpointSupport.HasRole(current, "1")) return ApplicationResult.Forbidden();
        if(request.Id==current.UserId)return ApplicationResult.Conflict(new {message="No puede cambiar el estado de su propia cuenta."});
        var item=await db.Users.FirstOrDefaultAsync(x=>x.Id==request.Id,ct);if(item==null)return ApplicationResult.NotFound();
        if(item.Role==UserRole.SUPERADMIN)return ApplicationResult.Conflict(new {message="La suspensión de un superadministrador requiere un proceso administrativo separado."});
        if(request.Input.Status==AccountStatus.ACTIVO && item.ValidUntil<=DateTime.UtcNow)return ApplicationResult.Conflict(new {message="La cuenta está vencida. La reactivación no sustituye la renovación por compra."});
        item.Status=request.Input.Status;item.AccountStatus=item.Status;EndpointSupport.Audit(db,current,"USER_STATUS_CHANGED","User",item.Id,item.Status.ToString());await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class SetUserStatusCommandValidator : AbstractValidator<SetUserStatusCommand>
{
    public SetUserStatusCommandValidator()
    {
        RuleFor(x=>x.Id).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Input).NotNull().SetValidator(new UserStatusInputValidator());
    }
}
