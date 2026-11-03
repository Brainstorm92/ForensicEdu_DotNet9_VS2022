namespace ForensicEdu.Application.Common.Interfaces;
public sealed record VideoPlayback(string Url,DateTime ExpiresAt,bool PrivateVideo=true);
public sealed record VideoProbe(long Size,bool IsMp4);
public interface IPrivateVideoStorage
{
 Task<string> UploadUrlAsync(string stagingKey,DateTime expires,CancellationToken ct);
 Task<VideoProbe?> ProbeAsync(string key,CancellationToken ct);
 Task PublishAsync(string stagingKey,string key,CancellationToken ct);
 Task<VideoPlayback> PlaybackAsync(string key,CancellationToken ct);
 Task DeleteAsync(string key,CancellationToken ct);
 Task CleanStagingAsync(CancellationToken ct);
}
public sealed record VideoUploadClaim(string UserId,string CourseId,string ModuleId,string StagingKey,string Key,long Size,string PreviousSource,DateTime ExpiresAt);
public interface IVideoUploadTokens {string Create(VideoUploadClaim claim);VideoUploadClaim? Read(string token);}
public sealed record VideoUploadInput(string FileName,long Size);
public sealed record VideoUploadFinish(string Token);
public interface IVideoAdministration
{
 Task<ForensicEdu.Application.Common.Results.ApplicationResult> BeginAsync(string courseId,string moduleId,VideoUploadInput input,CancellationToken ct);
 Task<ForensicEdu.Application.Common.Results.ApplicationResult> FinishAsync(string courseId,string moduleId,string token,CancellationToken ct);
 Task<ForensicEdu.Application.Common.Results.ApplicationResult> CancelAsync(string courseId,string moduleId,string token,CancellationToken ct);
 Task<ForensicEdu.Application.Common.Results.ApplicationResult> PreviewAsync(string courseId,string moduleId,CancellationToken ct);
 Task<ForensicEdu.Application.Common.Results.ApplicationResult> RemoveAsync(string courseId,string moduleId,CancellationToken ct);
}
