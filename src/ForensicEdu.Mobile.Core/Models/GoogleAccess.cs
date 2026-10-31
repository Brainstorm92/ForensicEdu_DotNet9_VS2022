namespace ForensicEdu.Mobile.Core.Models;
public sealed record GoogleConfiguration(bool Enabled,string ClientId,string Nonce);
public sealed record GoogleAccessResult(string? Token,bool RequiresRegistration,bool RequiresLinkPassword,string Email,string Nombres,string Apellidos,string Message);
public sealed record GoogleProfile(string Nombres,string Apellidos,string Dni,string Phone,string TipoDocumento,string PaisResidencia,string NivelEducativo,string InstitucionEducativa,string Profesion,bool AcceptTerms,bool AcceptPrivacy);
