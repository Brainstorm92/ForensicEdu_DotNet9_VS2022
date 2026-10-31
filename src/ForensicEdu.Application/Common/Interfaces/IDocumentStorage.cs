namespace ForensicEdu.Application.Common.Interfaces;
public interface IDocumentStorage
{
 Task SaveAsync(string key, byte[] bytes, string contentType, CancellationToken ct);
 Task<byte[]?> ReadAsync(string key, CancellationToken ct);
 Task DeleteAsync(string key, CancellationToken ct);
}
