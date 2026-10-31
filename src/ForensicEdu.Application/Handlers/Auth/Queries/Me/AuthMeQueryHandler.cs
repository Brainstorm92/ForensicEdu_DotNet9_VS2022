using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Auth.Queries.Me;

public sealed class AuthMeQueryHandler(IApplicationDbContext _context, ICurrentUser current) : IRequestHandler<AuthMeQuery, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AuthMeQuery message, CancellationToken cancellationToken)
    {


        var id = current.UserId;
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return ApplicationResult.Unauthorized();
        if (user.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION || user.ValidUntil <= DateTime.UtcNow)
            return ApplicationResult.Forbidden( new { message = "Su cuenta no tiene acceso vigente." });
        return ApplicationResult.Success(new { user.Id, user.Nombres, user.Apellidos, user.FullName, user.Email, user.Dni, user.Phone,
            user.TipoDocumento, user.PaisResidencia, user.NivelEducativo, user.InstitucionEducativa, user.Profesion,
            user.Role, user.Status, user.RegisteredAt, user.ValidUntil, user.HasApprovedPurchase, user.LastPurchaseDate });
    }
}
