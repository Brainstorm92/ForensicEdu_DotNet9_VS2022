using ForensicEdu.Application.Common.Interfaces;
namespace ForensicEdu.Infrastructure.BackgroundJobs;
public sealed class VideoStagingCleanupJob(IPrivateVideoStorage storage){public Task ExecuteAsync()=>storage.CleanStagingAsync(CancellationToken.None);}
