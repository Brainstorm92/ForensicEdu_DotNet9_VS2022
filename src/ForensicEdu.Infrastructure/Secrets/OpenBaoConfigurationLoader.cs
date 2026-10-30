using System.Net.Http.Json;
using System.Text.Json;

namespace ForensicEdu.Infrastructure.Secrets;

// Bootstrap configuration is loaded before DI: database and authentication registrations need it.
public sealed class OpenBaoConfigurationLoader(HttpClient client)
{
    public async Task<string> LoadAsync(OpenBaoOptions options, CancellationToken cancellationToken = default)
    {
        Validate(options);
        try
        {
            var address = new Uri(options.Address.TrimEnd('/') + "/");
            using var login = await client.PostAsJsonAsync(new Uri(address,
                $"v1/auth/{EncodePath(options.AuthMount)}/login"),
                new { role_id = options.RoleId, secret_id = options.SecretId }, cancellationToken);
            if (!login.IsSuccessStatusCode)
                throw new InvalidOperationException($"OpenBao: autenticación AppRole rechazada (HTTP {(int)login.StatusCode}).");
            using var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync(cancellationToken));
            var token = loginJson.RootElement.GetProperty("auth").GetProperty("client_token").GetString();
            if (string.IsNullOrWhiteSpace(token) || token.Contains('\r') || token.Contains('\n'))
                throw new JsonException();
            using var request = new HttpRequestMessage(HttpMethod.Get,
                new Uri(address, $"v1/{EncodePath(options.Mount)}/data/{EncodePath(options.SecretPath)}"));
            request.Headers.Add("X-Vault-Token", token);
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"OpenBao: lectura del secreto rechazada (HTTP {(int)response.StatusCode}).");
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var data = json.RootElement.GetProperty("data").GetProperty("data");
            if (data.ValueKind != JsonValueKind.Object) throw new JsonException();
            // Bootstrap credentials and endpoints must never be replaced by the secret being read.
            foreach (var property in data.EnumerateObject())
                if (property.Name.Equals("OpenBao", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.StartsWith("OpenBao:", StringComparison.OrdinalIgnoreCase))
                    throw new JsonException();
            return data.GetRawText();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("OpenBao: tiempo de conexión agotado; no se inició la API.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("OpenBao: no fue posible conectar de forma segura; no se inició la API.");
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException &&
            !ex.Message.StartsWith("OpenBao:", StringComparison.Ordinal))
        {
            // Do not attach the response, original exception or any secret to logs.
            throw new InvalidOperationException("OpenBao: respuesta inválida; se esperaba un secreto KV v2 con configuración JSON.");
        }
    }

    private static string EncodePath(string value) => string.Join("/", value.Split('/').Select(Uri.EscapeDataString));

    private static void Validate(OpenBaoOptions options)
    {
        if (!Uri.TryCreate(options.Address, UriKind.Absolute, out var address) ||
            address.Scheme != Uri.UriSchemeHttps || address.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(address.UserInfo) || !string.IsNullOrEmpty(address.Query) ||
            !string.IsNullOrEmpty(address.Fragment))
            throw new InvalidOperationException("OpenBao: Address debe ser una URL HTTPS base sin credenciales ni ruta.");
        foreach (var path in new[] { options.Mount, options.AuthMount, options.SecretPath })
            if (string.IsNullOrWhiteSpace(path) || path.Split('/').Any(p =>
                p.Length == 0 || p is "." or ".." || p.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_')))
                throw new InvalidOperationException("OpenBao: ruta de autenticación o secreto inválida.");
        if (string.IsNullOrWhiteSpace(options.RoleId) || string.IsNullOrWhiteSpace(options.SecretId))
            throw new InvalidOperationException("OpenBao: configure RoleId y SecretId fuera del repositorio.");
    }
}
