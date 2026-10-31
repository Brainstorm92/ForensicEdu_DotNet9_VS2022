using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record ProfileInput(string Nombres, string Apellidos, string Phone, string PaisResidencia,
    string NivelEducativo, string InstitucionEducativa, string Profesion);
