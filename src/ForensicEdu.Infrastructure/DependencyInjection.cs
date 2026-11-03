using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Resilience;
using ForensicEdu.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
        => services.AddPostgreSqlPersistence(configuration).AddDatabaseResilience(configuration).AddPlatformAdapters().AddImageStorage(configuration).AddGoogleSignIn(configuration).AddPlatformEmail();
}
