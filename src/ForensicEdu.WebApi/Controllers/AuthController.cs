using System;
using System.Linq;
using System.Threading.Tasks;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Domain.Entities;
using ForensicEdu.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ForensicEdu.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AuthController(IApplicationDbContext context)
    {
        _context = context;
    }

    private string CreateToken(User user)
    {
        var config = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var claims = new[] {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, ((int)user.Role).ToString())
        };
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!));
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"], audience: config["JwtSettings:Audience"],
            claims: claims, expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string FullName, string Email, string Dni, string Phone, string Password,
        string Nombres = "", string Apellidos = "", string TipoDocumento = "DNI", string PaisResidencia = "",
        string NivelEducativo = "", string InstitucionEducativa = "", string Profesion = "",
        bool AcceptTerms = false, bool AcceptPrivacy = false);

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return Unauthorized();
        if (user.Status is AccountStatus.SUSPENDIDO or AccountStatus.ELIMINADO or AccountStatus.PENDIENTE_DE_ELIMINACION || user.ValidUntil <= DateTime.UtcNow)
            return StatusCode(403, new { message = "Su cuenta no tiene acceso vigente." });
        return Ok(new { user.Id, user.Nombres, user.Apellidos, user.FullName, user.Email, user.Dni, user.Phone,
            user.TipoDocumento, user.PaisResidencia, user.NivelEducativo, user.InstitucionEducativa, user.Profesion,
            user.Role, user.Status, user.RegisteredAt, user.ValidUntil, user.HasApprovedPurchase, user.LastPurchaseDate });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            return Unauthorized(new { message = "Ingrese correo y contraseña." });
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Credenciales incorrectas o usuario no registrado." });
        }

        if (user.Status == AccountStatus.SUSPENDIDO || user.Status == AccountStatus.ELIMINADO || user.Status == AccountStatus.PENDIENTE_DE_ELIMINACION || (user.ValidUntil.HasValue && user.ValidUntil <= DateTime.UtcNow))
        {
            return StatusCode(403, new { message = "Su cuenta no tiene acceso vigente. Contacte a soporte." });
        }

        return Ok(new
        {
            token = CreateToken(user),
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || !System.Net.Mail.MailAddress.TryCreate(request.Email, out _)
            || string.IsNullOrWhiteSpace(request.Dni) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            return BadRequest(new { message = "Complete nombre, correo válido, documento y contraseña de al menos 12 caracteres." });
        if (!request.AcceptTerms || !request.AcceptPrivacy)
            return BadRequest(new { message = "Debe aceptar los términos y el tratamiento de datos personales." });
        if (string.IsNullOrWhiteSpace(request.Nombres) || string.IsNullOrWhiteSpace(request.Apellidos) ||
            string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.PaisResidencia) ||
            string.IsNullOrWhiteSpace(request.InstitucionEducativa) || string.IsNullOrWhiteSpace(request.Profesion) ||
            request.TipoDocumento is not ("DNI" or "CE" or "Pasaporte") ||
            request.NivelEducativo is not ("Pregrado" or "Bachiller" or "Licenciatura" or "Maestría" or "Doctorado"))
            return BadRequest(new { message = "Complete los datos personales y académicos." });
        if (request.Nombres.Length > 100 || request.Apellidos.Length > 100 || request.Email.Length > 200 || request.Dni.Length > 20 || request.Password.Length > 72)
            return BadRequest(new { message = "Revise la longitud de los campos. La contraseña admite hasta 72 caracteres." });
        var email = request.Email.Trim().ToLowerInvariant();
        var document = request.Dni.Trim().ToUpperInvariant();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email || u.Dni == document);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe un usuario con este correo electrónico o DNI." });
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
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
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            return Conflict(new { message = "No fue posible registrar la cuenta. Compruebe si el correo ya está registrado." });
        }

        return Ok(new { message = "Usuario registrado exitosamente. Cuenta activa por 30 días para realizar su primera compra.", userId = user.Id });
    }
}
