namespace Infrastructure.Configurations;

public class ConfigurationUpdateSettings
{
    public string ConfigurationProviderUri { get; set; } = string.Empty;

    public long UpdateIntervalInMinutes { get; set; } = 10;
}