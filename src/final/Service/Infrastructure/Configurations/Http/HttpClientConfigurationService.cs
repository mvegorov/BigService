using System.Text;
using System.Text.Json;

namespace Infrastructure.Configurations.Http;

public class HttpClientConfigurationService : IConfigurationService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    public HttpClientConfigurationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedConfigurationResponse?> GetConfigurationsAsync(int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/configurations?pageSize={pageSize}&pageToken={pageToken}", cancellationToken);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<PagedConfigurationResponse>(content, _jsonOptions);
    }

    public async Task CreateConfigurationAsync(ConfigurationItem configuration, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(configuration), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync("/configurations", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteConfigurationAsync(string key, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"/configurations/{key}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
