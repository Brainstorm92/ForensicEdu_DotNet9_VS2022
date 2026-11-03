namespace ForensicEdu.Application.Contracts.Auth;
public sealed record GoogleProfileInput(string Nombres,string Apellidos,string Dni,string Phone,string TipoDocumento,string PaisResidencia,string NivelEducativo,string InstitucionEducativa,string Profesion,bool AcceptTerms,bool AcceptPrivacy,DateOnly? BirthDate=null);
