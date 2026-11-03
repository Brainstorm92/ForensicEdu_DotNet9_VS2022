using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Auth;

public record RegisterRequest(string FullName, string Email, string Dni, string Phone, string Password,
        string Nombres = "", string Apellidos = "", string TipoDocumento = "DNI", string PaisResidencia = "",
        string NivelEducativo = "", string InstitucionEducativa = "", string Profesion = "",
        bool AcceptTerms = false, bool AcceptPrivacy = false, DateOnly? BirthDate = null);
