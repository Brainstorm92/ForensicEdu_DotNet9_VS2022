namespace ForensicEdu.Application.Contracts.Auth;
public sealed record GoogleAuthResponse(string? Token=null,bool RequiresRegistration=false,bool RequiresLinkPassword=false,string Email="",string Nombres="",string Apellidos="",string Message="");
