using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
