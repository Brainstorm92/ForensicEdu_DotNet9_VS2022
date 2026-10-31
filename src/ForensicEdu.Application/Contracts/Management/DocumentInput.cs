using ForensicEdu.Domain.Enums;
namespace ForensicEdu.Application.Contracts.Management;

public record DocumentInput(string Title, string StorageUrl, string FileType = "PDF", string FileSize = "", bool RequireWatermark = false);
