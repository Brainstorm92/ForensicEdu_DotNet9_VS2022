using ForensicEdu.Domain.Entities;
namespace ForensicEdu.Application.Common.Interfaces;

public interface IVoucherStorage
{
    Task<ForensicEdu.Application.Common.Results.DownloadContent?> ReadAsync(string storageName, CancellationToken cancellationToken);
    Task<string> SaveAsync(byte[] bytes, string extension, CancellationToken cancellationToken);
}
