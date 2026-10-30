using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Infrastructure.BackgroundJobs;
using ForensicEdu.Infrastructure.Persistence;
using ForensicEdu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ForensicEdu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Support both SQL Server and PostgreSQL, with in-memory fallback for local dev without a DB server
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

        if (provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            // InMemory for immediate prototyping/unit testing
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ForensicEduDb"));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("No se cargó la conexión PostgreSQL. Ejecute WebApi con el perfil http (Development) y configure ConnectionStrings:DefaultConnection mediante Administrar secretos de usuario.");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, b => b
                    .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), null)));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfCertificateService, PdfCertificateService>();
        services.AddScoped<AccountLifecycleHangfireJob>();

        return services;
    }
}
