namespace ForensicEdu.Application.Common.Interfaces;
public interface IImageProcessor
{
 Task<byte[]> NormalizeAsync(byte[] bytes,CancellationToken ct);
}
