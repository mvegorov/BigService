using Refit;

namespace Infrastructure.Configurations.Refit;

public interface IRefitConfigurationApi
{
    [Get("/configurations")]
    Task<PagedConfigurationResponse> GetConfigurationsAsync([Query] int pageSize, [Query] string? pageToken);

    [Post("/configurations")]
    Task CreateConfigurationAsync([Body] ConfigurationItem configuration);

    [Delete("/configurations/{key}")]
    Task DeleteConfigurationAsync(string key);
}