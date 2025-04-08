namespace Infrastructure.Configurations;

public interface IConfigurationService
{
    Task<PagedConfigurationResponse?> GetConfigurationsAsync(int pageSize, string? pageToken, CancellationToken cancellationToken);

    Task CreateConfigurationAsync(ConfigurationItem configuration, CancellationToken cancellationToken);

    Task DeleteConfigurationAsync(string key, CancellationToken cancellationToken);
}
