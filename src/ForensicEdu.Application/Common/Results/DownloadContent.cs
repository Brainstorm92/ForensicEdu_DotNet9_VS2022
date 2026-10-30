namespace ForensicEdu.Application.Common.Results;

public sealed record DownloadContent(byte[] Bytes, string ContentType, string FileName);
