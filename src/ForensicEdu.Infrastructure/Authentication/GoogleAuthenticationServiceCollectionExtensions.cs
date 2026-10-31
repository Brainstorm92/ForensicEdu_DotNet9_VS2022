using ForensicEdu.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace ForensicEdu.Infrastructure;
public static class GoogleAuthenticationServiceCollectionExtensions
{
 public static IServiceCollection AddGoogleSignIn(this IServiceCollection services,IConfiguration config){services.Configure<Authentication.GoogleSignInOptions>(config.GetSection("Authentication:Google"));services.AddSingleton<IGoogleIdentityValidator,Authentication.GoogleIdentityValidator>();return services;}
}
