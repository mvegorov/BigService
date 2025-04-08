using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Configurations;

public class ConfigurationsUpdateBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ConfigurationsUpdateBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await DoInitialUpdate(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        MyConfigurationProviderService myConfigurationProviderService = scope.ServiceProvider.GetRequiredService<MyConfigurationProviderService>();
        MyConfigurationProvider myProvider = scope.ServiceProvider.GetRequiredService<MyConfigurationProvider>();

        await myConfigurationProviderService.StartIntervalConfigurationUpdateAsync(
            myProvider,
            stoppingToken);
    }

    private async Task DoInitialUpdate(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        MyConfigurationProviderService myConfigurationProviderService = scope.ServiceProvider.GetRequiredService<MyConfigurationProviderService>();
        MyConfigurationProvider myConfigurationProvider = scope.ServiceProvider.GetRequiredService<MyConfigurationProvider>();

        await myConfigurationProviderService.LoadConfigurationsAsync(stoppingToken);
        myConfigurationProvider.Load();
    }
}