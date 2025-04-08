using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configurations;

public class MyConfigurationProvider : ConfigurationProvider
{
    private readonly MyConfigurationProviderService _updateService;

    public MyConfigurationProvider(MyConfigurationProviderService updateService)
    {
        _updateService = updateService;
    }

    public IDictionary<string, string?> GetCurrentConfigurations()
    {
        return Data;
    }

    public override void Load()
    {
        IDictionary<string, string?> configData = _updateService.GetCurrentConfigurations();
        if (IsConfigurationChanged(configData))
        {
            Data = configData;
            OnReload();
        }
    }

    private bool IsConfigurationChanged(IDictionary<string, string?> newConfigurations)
    {
        if (Data.Count != newConfigurations.Count)
        {
            return true;
        }

        foreach (KeyValuePair<string, string?> kvp in newConfigurations)
        {
            if (!Data.ContainsKey(kvp.Key) || Data[kvp.Key] != kvp.Value)
            {
                return true;
            }
        }

        return false;
    }
}
