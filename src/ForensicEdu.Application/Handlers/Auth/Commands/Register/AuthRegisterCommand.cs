using FluentValidation;
using MediatR;
using System.Text.Json;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application.Common.Results;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ForensicEdu.Application.Contracts.Auth;

namespace ForensicEdu.Application.Handlers.Auth.Commands.Register;
public sealed record AuthRegisterCommand(RegisterRequest Request) : IRequest<ApplicationResult>;
public sealed class AuthRegisterCommandHandler(IApplicationDbContext _context, IPasswordService passwords, IPersistenceErrors errors) : IRequestHandler<AuthRegisterCommand, ApplicationResult>
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
            Email = email,
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

        return ApplicationResult.Success(new { message = "Usuario registrado exitosamente. Cuenta activa por 30 días para realizar su primera compra.", userId = user.Id });
    }
}

public sealed class AuthRegisterCommandValidator : AbstractValidator<AuthRegisterCommand>
{
    public AuthRegisterCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request != null, () => {
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Request.Dni).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Password).NotEmpty().MinimumLength(12).MaximumLength(72);
        RuleFor(x => x.Request.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Request.PaisResidencia).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.InstitucionEducativa).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Profesion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.TipoDocumento).Must(v => v is "DNI" or "CE" or "Pasaporte");
        RuleFor(x => x.Request.NivelEducativo).Must(v => v is "Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado");
        RuleFor(x => x.Request.AcceptTerms).Equal(true);
        RuleFor(x => x.Request.AcceptPrivacy).Equal(true);
        });
    }
}
