using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Login;

public sealed class AuthLoginCommandHandler(IApplicationDbContext _context, ITokenService tokens, IPasswordService passwords) : IRequestHandler<AuthLoginCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AuthLoginCommand message, CancellationToken cancellationToken)
    {
        var request = message.Request;

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email || (u.Username != null && u.Username.ToLower() == email));
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !passwords.Verify(request.Password, user.PasswordHash))
        {
            return ApplicationResult.Unauthorized(new { message = "Credenciales incorrectas o usuario no registrado." });
        }

        if (user.Status == AccountStatus.SUSPENDIDO || user.Status == AccountStatus.ELIMINADO || user.Status == AccountStatus.PENDIENTE_DE_ELIMINACION || (user.ValidUntil.HasValue && user.ValidUntil <= DateTime.UtcNow))
        {
            return ApplicationResult.Forbidden( new { message = "Su cuenta no tiene acceso vigente. Contacte a soporte." });
        }

        if (user.EmailConfirmationRequired && !user.EmailConfirmedAt.HasValue)
            return ApplicationResult.Forbidden(new { message = "Confirma tu correo antes de iniciar sesión. Utiliza Reenviar confirmación si necesitas un nuevo enlace.", requiresEmailConfirmation = true });
        return ApplicationResult.Success(new
        {
            token = tokens.Create(user),
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Dni,
                user.Role,
                user.Status,
                user.RegisteredAt,
                user.ValidUntil,
                user.HasApprovedPurchase
            }
        });
    }
}
