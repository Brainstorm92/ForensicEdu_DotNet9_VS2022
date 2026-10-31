namespace ForensicEdu.Infrastructure.Secrets;

public sealed class OpenBaoOptions
{
    public bool Enabled { get; set; }
    public string Address { get; set; } = "";
    public string Mount { get; set; } = "forense";
    public string SecretPath { get; set; } = "produccion/api";
    public string AuthMount { get; set; } = "approle";
    public string RoleId { get; set; } = "";
    public string SecretId { get; set; } = "";
}
