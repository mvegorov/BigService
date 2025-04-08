namespace Infrastructure.Configurations.Refit;

public class RefitClientConfigurationService : IConfigurationService
{
    private readonly IRefitConfigurationApi _refitClient;

    public RefitClientConfigurationService(IRefitConfigurationApi refitClient)
    {
        _refitClient = refitClient;
    }

    public async Task<PagedConfigurationResponse?> GetConfigurationsAsync(int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        return await _refitClient.GetConfigurationsAsync(pageSize, pageToken);
    }

    public async Task CreateConfigurationAsync(ConfigurationItem configuration, CancellationToken cancellationToken)
    {
        await _refitClient.CreateConfigurationAsync(configuration);
    }

    public async Task DeleteConfigurationAsync(string key, CancellationToken cancellationToken)
    {
        await _refitClient.DeleteConfigurationAsync(key);
    }
}