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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
        if (!databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El proveedor de esta aplicación debe ser PostgreSQL.");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection para PostgreSQL mediante secretos o variables de entorno.");
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, b => b
            .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(3), null)));

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IVoucherStorage, PrivateVoucherStorage>();
        services.AddScoped<IPersistenceErrors, PersistenceErrors>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfCertificateService, PdfCertificateService>();
        services.AddScoped<AccountLifecycleHangfireJob>();

        return services;
    }
}
