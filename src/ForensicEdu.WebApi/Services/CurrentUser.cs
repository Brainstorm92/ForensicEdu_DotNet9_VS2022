using System.Security.Claims;
using ForensicEdu.Application.Common.Interfaces;
namespace ForensicEdu.WebApi.Services;
public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Se requiere una sesión autenticada.");
    public bool IsInRole(string role) => accessor.HttpContext?.User.IsInRole(role) == true;
}
