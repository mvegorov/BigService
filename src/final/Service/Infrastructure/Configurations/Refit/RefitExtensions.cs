using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Infrastructure.Configurations.Refit;

public static class RefitExtensions
{
    public static IServiceCollection AddRefitClientConfigurationService(this IServiceCollection services, Uri baseUri)
    {
        services.AddRefitClient<IRefitConfigurationApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                ConfigurationUpdateSettings settings = serviceProvider.GetRequiredService<IOptions<ConfigurationUpdateSettings>>().Value;
                client.BaseAddress = new Uri(settings.ConfigurationProviderUri);
            });
        services.AddTransient<IConfigurationService, RefitClientConfigurationService>();
        return services;
    }
}