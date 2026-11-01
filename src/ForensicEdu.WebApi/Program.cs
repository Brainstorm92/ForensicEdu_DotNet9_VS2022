using Microsoft.EntityFrameworkCore;
using ForensicEdu.Infrastructure.Secrets;
using ForensicEdu.WebApi.Common;
using ForensicEdu.WebApi.Services;
using ForensicEdu.Application.Common.Interfaces;
using ForensicEdu.Application;
using ForensicEdu.Infrastructure;
using ForensicEdu.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;

var startupArgs=args.Where(x=>x!="--upgrade-notifications" && x!="--upgrade-registration" && x!="--upgrade-advisories" && x!="--bootstrap-admin" && x!="--seed-demo" && x!="--provision-admin" && x!="--upgrade-media" && x!="--upgrade-google").ToArray();
var builder = WebApplication.CreateBuilder(startupArgs);
var protectionDirectory = new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys"));
protectionDirectory.Create();
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("ForensicEdu").PersistKeysToFileSystem(protectionDirectory);
if (OperatingSystem.IsWindows()) dataProtection.ProtectKeysWithDpapi();
builder.Services.AddEmailRateLimits();
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(startupArgs);
}

// Load KV v2 before database, storage and Google registrations. Fail closed if enabled.
await builder.Configuration.AddOpenBaoSecretsAsync();
// Explicit deployment overrides remain higher priority than the stored JSON.
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(startupArgs);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
// Use a per-process key locally; production must supply JwtSettings__Secret.
if (builder.Environment.IsDevelopment())
    builder.Configuration["JwtSettings:Secret"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("Configure JwtSettings__Secret con al menos 32 caracteres.");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"], ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events=new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {OnTokenValidated=TokenAccountGuard.ValidateAsync};
    });
builder.Services.AddAuthorization();
builder.Services.AddHostedService<ExpiredExamsWorker>();
builder.Services.AddOpenApi(options=>options.AddDocumentTransformer((document,context,ct)=>{
    document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
    document.Components.SecuritySchemes["Bearer"]=new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
        Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT",Description="Token obtenido en POST /api/auth/login." };
    return Task.CompletedTask;
}));
builder.Services.AddOpenApi(options=>options.AddOperationTransformer((operation,context,ct)=>{
    if(context.Description.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>().Any())
        operation.Security.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
            [new Microsoft.OpenApi.Models.OpenApiSecurityScheme {Reference=new Microsoft.OpenApi.Models.OpenApiReference {Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id="Bearer"}}]=Array.Empty<string>() });
    return Task.CompletedTask;
}));

// Add Clean Architecture Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Hangfire for background scheduled jobs (Account expiration checks at midnight)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

// Add CORS to communicate directly with React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
if(args.Contains("--upgrade-notifications"))
{
    using var scope=app.Services.CreateScope();
    await ForensicEdu.Infrastructure.Persistence.StudentNotificationsSchema.ApplyAsync(scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>());
    var count=await scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>().StudentNotifications.AsNoTracking().CountAsync();
    Console.WriteLine($"Centro de notificaciones preparado y lectura verificada. Avisos existentes: {count}. No se enviaron correos ni se modificaron cuentas.");return;
}
if(args.Contains("--upgrade-registration"))
{
    using var scope=app.Services.CreateScope();
    await ForensicEdu.Infrastructure.Persistence.RegistrationEmailSchema.ApplyAsync(scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>());
    Console.WriteLine("Registro actualizado. Cuentas existentes conservan acceso; fechas de nacimiento no se inventaron.");return;
}
if(args.Contains("--upgrade-advisories"))
{
    using var scope=app.Services.CreateScope();
    await ForensicEdu.Infrastructure.Persistence.AdvisoryCareSchema.ApplyAsync(scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>());
    Console.WriteLine("Seguimiento de asesorías preparado. Se conservaron compras existentes y se crearon las atenciones faltantes.");return;
}
if(args.Contains("--upgrade-google"))
{
    using var scope=app.Services.CreateScope();
    var database=scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>();
    await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(database.Database,
        "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"GoogleSubject\" character varying(255) NULL; CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Users_GoogleSubject\" ON \"Users\" (\"GoogleSubject\");");
    Console.WriteLine("Acceso con Google preparado. No se modificaron cuentas existentes.");return;
}
if(args.Contains("--upgrade-media"))
{
    using var scope=app.Services.CreateScope();
    var database=scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>();
    await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(database.Database,
        "ALTER TABLE \"Courses\" ADD COLUMN IF NOT EXISTS \"InstructorImageUrl\" text NOT NULL DEFAULT ''; ");
    Console.WriteLine("Esquema de imágenes actualizado. Se conservaron los registros existentes.");return;
}
if(args.Contains("--provision-admin"))
{
    using var scope=app.Services.CreateScope();
    var database=scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.ApplicationDbContext>();
    await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(database.Database,
        "ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"Username\" character varying(50) NULL; CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Users_Username\" ON \"Users\" (\"Username\");");
    await ForensicEdu.Infrastructure.Services.AdministratorProvisioner.CreateAsync(database,scope.ServiceProvider.GetRequiredService<IPasswordService>(),builder.Configuration["AdminProvision:Username"],builder.Configuration["AdminProvision:Password"]);
    return;
}
if(args.Contains("--bootstrap-admin"))
{
    using var scope=app.Services.CreateScope();
    await ForensicEdu.Infrastructure.Services.FirstAdministrator.CreateAsync(
        scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(),scope.ServiceProvider.GetRequiredService<IPasswordService>(),
        builder.Configuration["BootstrapAdmin:Email"],builder.Configuration["BootstrapAdmin:Password"]);
    Console.WriteLine("Primer administrador creado. Inicie sesión mediante /api/auth/login.");
    return;
}
if(args.Contains("--seed-demo"))
{
    if(!app.Environment.IsDevelopment())throw new InvalidOperationException("Los datos de prueba solo se generan en Development.");
    using var scope=app.Services.CreateScope();
    var added=await scope.ServiceProvider.GetRequiredService<ForensicEdu.Infrastructure.Persistence.DevelopmentDataSeeder>().SeedAsync();
    Console.WriteLine($"Productos de prueba añadidos: {added}. No se modificaron usuarios, pagos ni registros existentes.");
    return;
}
app.UseExceptionHandler();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    var dbConnection = new Npgsql.NpgsqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
    app.Logger.LogInformation("Destino PostgreSQL: {Host}:{Port}/{Database}", dbConnection.Host, dbConnection.Port, dbConnection.Database);
    app.MapGet("/api/health/database", async (ForensicEdu.Infrastructure.Persistence.ApplicationDbContext db) =>
    {
        try
        {
            var connected = await db.Database.CanConnectAsync();
            return Results.Json(new { connected, dbConnection.Host, dbConnection.Port, dbConnection.Database }, statusCode: connected ? 200 : 503);
        }
        catch { return Results.Json(new { connected = false, dbConnection.Host, dbConnection.Port, dbConnection.Database }, statusCode: 503); }
    });
    app.MapOpenApi();
    app.MapScalarApiReference(); // Modern Scalar UI for .NET 9
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontendApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Hangfire Dashboard (Admin only in production)
app.MapHangfireDashboard("/hangfire").RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "1" });

// Register recurring jobs: Daily Account Lifecycle check at 00:00 UTC
RecurringJob.AddOrUpdate<AccountLifecycleHangfireJob>(
    "daily-account-lifecycle-audit",
    job => job.ExecuteDailyLifecycleAuditAsync(),
    Cron.Daily(0, 0)
);

RecurringJob.AddOrUpdate<ForensicEdu.Infrastructure.BackgroundJobs.NotificationEmailJob>(
    "student-notification-emails", job => job.ExecuteAsync(), "*/2 * * * *");
app.MapControllers();
// Keep the SPA separate from API and documentation routes.

app.MapFallbackToFile("/app/{*path:nonfile}", "index.html");

app.Run();
