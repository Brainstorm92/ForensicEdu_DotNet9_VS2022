namespace ForensicEdu.Application.Common.Interfaces;
public interface IImageStorage
{
 Task SaveAsync(string id,byte[] bytes,CancellationToken ct);
 Task<byte[]?> ReadAsync(string id,CancellationToken ct);
}
