using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Configurations.Http;

public static class HttpExtensions
{
    public static IServiceCollection AddHttpClientConfigurationService(this IServiceCollection services)
    {
        services.AddHttpClient<IConfigurationService, HttpClientConfigurationService>((serviceProvider, client) =>
        {
            ConfigurationUpdateSettings settings = serviceProvider.GetRequiredService<IOptions<ConfigurationUpdateSettings>>().Value;
            client.BaseAddress = new Uri(settings.ConfigurationProviderUri);
        });
        return services;
    }
}