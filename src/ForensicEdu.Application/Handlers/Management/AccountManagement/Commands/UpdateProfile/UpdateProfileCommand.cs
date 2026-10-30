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
namespace ForensicEdu.Application.Handlers.Management.AccountManagement.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(ProfileInput Input) : IRequest<ApplicationResult>;
public sealed class UpdateProfileCommandHandler(IApplicationDbContext db, ICurrentUser current) : IRequestHandler<UpdateProfileCommand,ApplicationResult>
{
    public async Task<ApplicationResult> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var item=await db.Users.FirstOrDefaultAsync(x=>x.Id==current.UserId,ct);if(item==null)return ApplicationResult.NotFound();
        var input=request.Input;item.Nombres=input.Nombres.Trim();item.Apellidos=input.Apellidos.Trim();item.FullName=$"{item.Nombres} {item.Apellidos}";
        item.Phone=input.Phone;item.Telefono=input.Phone;item.PaisResidencia=input.PaisResidencia;item.NivelEducativo=input.NivelEducativo;
        item.InstitucionEducativa=input.InstitucionEducativa;item.Profesion=input.Profesion;
        EndpointSupport.Audit(db,current,"PROFILE_UPDATED","User",item.Id);await db.SaveChangesAsync(ct);return ApplicationResult.NoContent();
    }
}

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x=>x.Input).NotNull().SetValidator(new ProfileInputValidator());
    }
}
