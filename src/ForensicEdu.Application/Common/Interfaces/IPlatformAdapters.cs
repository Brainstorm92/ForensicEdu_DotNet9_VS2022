using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;

public interface ICurrentUser
{
    string UserId { get; }
    bool IsInRole(string role);
}
public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
public interface ITokenService { string Create(User user); }
public interface IVoucherStorage
{
    Task<ForensicEdu.Application.Common.Results.DownloadContent?> ReadAsync(string storageName, CancellationToken cancellationToken);
    Task<string> SaveAsync(byte[] bytes, string extension, CancellationToken cancellationToken);
}
public interface IPersistenceErrors { bool IsUniqueViolation(Exception exception); }
