using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace Infrastructure.Configurations;

public class MyConfigurationProviderService
{
    private readonly IConfigurationService _configurationService;

    private readonly ConfigurationUpdateSettings _settings;

    private Collection<KeyValuePair<string, string?>> CurrentConfigurations { get; set; }

    public MyConfigurationProviderService(
        IConfigurationService configurationService,
        IOptions<ConfigurationUpdateSettings> settings)
    {
        _configurationService = configurationService;
        _settings = settings.Value;
        CurrentConfigurations = new Collection<KeyValuePair<string, string?>>();
    }

    public MyConfigurationProviderService(
        IConfigurationService configurationService,
        ConfigurationUpdateSettings settings)
    {
        _configurationService = configurationService;
        _settings = settings;
        CurrentConfigurations = new Collection<KeyValuePair<string, string?>>();
    }

    public IDictionary<string, string?> GetCurrentConfigurations()
    {
        return CurrentConfigurations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public async Task StartIntervalConfigurationUpdateAsync(MyConfigurationProvider configurationProvider, CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.UpdateIntervalInMinutes));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await LoadConfigurationsAsync(cancellationToken);
            configurationProvider.Load();
        }
    }

    public async Task LoadConfigurationsAsync(CancellationToken cancellationToken, int pageSize = 10)
    {
        var configurations = new Collection<KeyValuePair<string, string?>>();
        string? pageToken = null;

        while (true)
        {
            PagedConfigurationResponse? response = await _configurationService.GetConfigurationsAsync(pageSize, pageToken, cancellationToken);

            if (response == null)
            {
                break;
            }

            if (response.Items != null)
            {
                foreach (ConfigurationItem item in response.Items)
                {
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        configurations.Add(new KeyValuePair<string, string?>(item.Key, item.Value));
                    }
                }
            }

            pageToken = response.PageToken;
            if (pageToken == null)
            {
                break;
            }
        }

        CurrentConfigurations = configurations;
    }
}