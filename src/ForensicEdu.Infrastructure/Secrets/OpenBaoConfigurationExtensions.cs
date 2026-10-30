using System.Text;
using Microsoft.Extensions.Configuration;

namespace ForensicEdu.Infrastructure.Secrets;

public static class OpenBaoConfigurationExtensions
{
    public static async Task AddOpenBaoSecretsAsync(this ConfigurationManager configuration,
        CancellationToken cancellationToken = default)
    {
        var options = configuration.GetSection("OpenBao").Get<OpenBaoOptions>() ?? new();
        if (!options.Enabled) return;
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
        var json = await new OpenBaoConfigurationLoader(client).LoadAsync(options, cancellationToken);
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            configuration.AddJsonStream(stream);
        }
        catch
        {
            throw new InvalidOperationException("OpenBao: el JSON del secreto no es una configuración .NET válida.");
        }
    }
}
