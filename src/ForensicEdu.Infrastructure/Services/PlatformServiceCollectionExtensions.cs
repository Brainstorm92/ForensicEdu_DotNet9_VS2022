using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure.Services;

public static class PlatformServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformAdapters(this IServiceCollection services)
    {
        services.AddScoped<ForensicEdu.Infrastructure.Persistence.DevelopmentDataSeeder>();
        services.AddScoped<IPasswordService,PasswordService>();
        services.AddScoped<ITokenService,TokenService>();
        services.AddScoped<IVoucherStorage,PrivateVoucherStorage>();
        services.AddScoped<IPersistenceErrors,PersistenceErrors>();
        services.AddScoped<IQrCodeService,QrCodeService>();
        services.AddScoped<IPdfCertificateService,PdfCertificateService>();
        services.AddScoped<AccountLifecycleHangfireJob>();
        services.AddScoped<IExpiredExamProcessor,ExpiredExamProcessor>();
        return services;
    }
}
