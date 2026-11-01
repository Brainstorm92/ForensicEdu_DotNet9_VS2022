using MediatR;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace ForensicEdu.Application.Handlers.Auth.Commands.Register;

public sealed class AuthRegisterCommandHandler(IApplicationDbContext _context, IPasswordService passwords, IPersistenceErrors errors, IEmailConfirmation confirmation) : IRequestHandler<AuthRegisterCommand, ApplicationResult>
{
    public async Task<ApplicationResult> Handle(AuthRegisterCommand message, CancellationToken cancellationToken)
    {
        var request = message.Request;

        var email = request.Email.Trim().ToLowerInvariant();
        var document = request.Dni.Trim().ToUpperInvariant();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email || u.Dni == document);
        if (exists)
        {
            return ApplicationResult.Invalid(new { message = "Ya existe un usuario con este correo electrónico o DNI." });
        }

        var user = new User
        {
            FullName = $"{request.Nombres.Trim()} {request.Apellidos.Trim()}",
            Nombres = request.Nombres.Trim(), Apellidos = request.Apellidos.Trim(),
            TipoDocumento = request.TipoDocumento, NumeroDocumento = document,
            PaisResidencia = request.PaisResidencia.Trim(), NivelEducativo = request.NivelEducativo,
            InstitucionEducativa = request.InstitucionEducativa.Trim(), Profesion = request.Profesion.Trim(),
            Email = email, BirthDate = request.BirthDate, EmailConfirmationRequired = true,
            Dni = document,
            Phone = request.Phone.Trim(), Telefono = request.Phone.Trim(),
            PasswordHash = passwords.Hash(request.Password),
            Role = UserRole.ESTUDIANTE,
            Status = AccountStatus.REGISTRADA,
            AccountStatus = AccountStatus.REGISTRADA,
            RegisteredAt = DateTime.UtcNow,
            HasApprovedPurchase = false,
            // 30 days initial grace period to perform first purchase
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        _context.Users.Add(user);
        _context.AuditLogs.Add(new AuditLog { UserId = user.Id, Action = "USER_REGISTERED", EntityType = "User", EntityId = user.Id,
            Details = "Registro web; aceptación de términos y consentimiento de tratamiento de datos personales." });
        try { await _context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (errors.IsUniqueViolation(ex))
        {
            return ApplicationResult.Conflict(new { message = "No fue posible registrar la cuenta. Compruebe si el correo ya está registrado." });
        }

        var delivery = await confirmation.SendInitialAsync(user,cancellationToken);
        var text = delivery==ConfirmationDelivery.Sent
            ? "Cuenta creada. Revisa tu correo y confirma tu dirección antes de iniciar sesión. El enlace dura 24 horas."
            : delivery==ConfirmationDelivery.Disabled
                ? "Cuenta creada, pendiente de confirmar. El envío de correos está desactivado. Cuando se active, utiliza Reenviar confirmación."
                : "Cuenta creada, pendiente de confirmar. No pudimos enviar la confirmación. Utiliza Reenviar confirmación en un minuto o contacta a soporte.";
        return ApplicationResult.Success(new { message=text, userId=user.Id, confirmationRequired=true, emailSent=delivery==ConfirmationDelivery.Sent });
    }
}
