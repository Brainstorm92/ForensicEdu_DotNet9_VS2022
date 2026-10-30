namespace ForensicEdu.Application.Common.Results;

public sealed record ApplicationResult(ResultOutcome Outcome, object? Value = null)
{
    public static ApplicationResult Success(object? value = null) => new(ResultOutcome.Success, value);
    public static ApplicationResult Invalid(object? value = null) => new(ResultOutcome.Invalid, value);
    public static ApplicationResult NotFound(object? value = null) => new(ResultOutcome.NotFound, value);
    public static ApplicationResult Forbidden(object? value = null) => new(ResultOutcome.Forbidden, value);
    public static ApplicationResult Unauthorized(object? value = null) => new(ResultOutcome.Unauthorized, value);
    public static ApplicationResult Conflict(object? value = null) => new(ResultOutcome.Conflict, value);
    public static ApplicationResult Unavailable(object? value = null) => new(ResultOutcome.Unavailable, value);
    public static ApplicationResult NoContent() => new(ResultOutcome.NoContent);
    public static ApplicationResult Download(byte[] bytes, string contentType, string fileName) => new(ResultOutcome.Download, new DownloadContent(bytes, contentType, fileName));
}
