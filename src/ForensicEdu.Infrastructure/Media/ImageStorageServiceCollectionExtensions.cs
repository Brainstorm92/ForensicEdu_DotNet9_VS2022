using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure;
public static class ImageStorageServiceCollectionExtensions
{
 public static IServiceCollection AddImageStorage(this IServiceCollection services,IConfiguration config){
  services.AddSingleton<IPrivateVideoStorage,Media.RustFsVideoStorage>();
  services.AddScoped<IVideoUploadTokens,Services.VideoUploadTokens>();
  services.AddScoped<BackgroundJobs.VideoStagingCleanupJob>();
  services.Configure<Media.RustFsOptions>(config.GetSection("ObjectStorage"));
  services.AddSingleton<IDocumentStorage,Documents.RustFsDocumentStorage>();services.AddSingleton<IDocumentProcessor,Documents.OfficeDocumentProcessor>();
  services.AddSingleton<IImageStorage,Media.RustFsImageStorage>();services.AddSingleton<IImageProcessor,Media.WebImageProcessor>();return services;
 }
}
