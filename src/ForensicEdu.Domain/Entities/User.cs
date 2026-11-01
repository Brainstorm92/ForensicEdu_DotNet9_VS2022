using System;
using System.Collections.Generic;
using ForensicEdu.Domain.Enums;

namespace ForensicEdu.Domain.Entities;

public partial class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "DNI";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string PaisResidencia { get; set; } = "Perú";
    public string? GoogleSubject { get; set; }
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public bool EmailConfirmationRequired { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }
    public string EmailConfirmationStamp { get; set; } = Guid.NewGuid().ToString();
    public DateTime? EmailConfirmationSentAt { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NivelEducativo { get; set; } = "Licenciatura";
    public string InstitucionEducativa { get; set; } = string.Empty;
    public string Profesion { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.ESTUDIANTE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public AccountStatus Status { get; set; } = AccountStatus.ACTIVO;
    public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVO;
    public DateTime? ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);
    public bool HasApprovedPurchase { get; set; } = false;
    public DateTime? LastPurchaseDate { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<DigitalCertificate> DigitalCertificates { get; set; } = new List<DigitalCertificate>();
}
