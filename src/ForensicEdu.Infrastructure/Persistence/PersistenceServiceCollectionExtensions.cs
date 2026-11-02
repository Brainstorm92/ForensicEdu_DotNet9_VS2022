using ForensicEdu.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlPersistence(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("DefaultConnection");
        var databaseProvider=configuration.GetValue<string>("DatabaseProvider")??"PostgreSQL";
        if(!databaseProvider.Equals("PostgreSQL",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("El proveedor debe ser PostgreSQL.");
        if(string.IsNullOrWhiteSpace(connectionString))throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection mediante secretos o variables de entorno.");
        // Do not retry SaveChanges implicitly: an ambiguous commit could duplicate a write.
        services.AddDbContext<ApplicationDbContext>(options=>options.UseNpgsql(connectionString,
            npgsql=>npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IApplicationDbContext>(provider=>provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICoursePurchaseGate,CoursePurchaseGate>();
        services.AddScoped<IUnitOfWork>(provider=>provider.GetRequiredService<ApplicationDbContext>());
        return services;
    }
}
