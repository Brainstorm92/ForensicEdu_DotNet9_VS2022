namespace ForensicEdu.Application.Contracts.Auth;
public sealed record GoogleSignInInput(string Credential,GoogleProfileInput? Profile=null,string? LinkPassword=null);
