using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }
    bool IsInRole(string role);
}
