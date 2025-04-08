using Infrastructure.Configurations;
using Microsoft.Extensions.Primitives;

namespace Infrastructure.Repositories.Connection;

public class ConnectionStringBuilder
{
    private readonly MyConfigurationProvider _configurationProvider;
    private IChangeToken _reloadToken;

    public ConnectionStringBuilder(MyConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
        _reloadToken = _configurationProvider.GetReloadToken();
    }

    public bool HasConfigurationsChanged() => _reloadToken.HasChanged;

    public string BuildConnectionString()
    {
        _reloadToken = _configurationProvider.GetReloadToken();
        IDictionary<string, string?> configurations = _configurationProvider.GetCurrentConfigurations();

        if (string.IsNullOrWhiteSpace(configurations["Host"]) ||
            string.IsNullOrWhiteSpace(configurations["Port"]) ||
            string.IsNullOrWhiteSpace(configurations["Database"]) ||
            string.IsNullOrWhiteSpace(configurations["Username"]) ||
            string.IsNullOrWhiteSpace(configurations["Password"]))
        {
            throw new InvalidOperationException("One or more required configuration values are missing.");
        }

        return $"Host={configurations["Host"]};" +
               $"Port={configurations["Port"]};" +
               $"Database={configurations["Database"]};" +
               $"Username={configurations["Username"]};" +
               $"Password={configurations["Password"]}";
    }
}