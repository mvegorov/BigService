using FluentMigrator.Runner;
using Infrastructure.Repositories.Connection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Migrations;

public class MigrationsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MigrationsBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        string connectionString = scope.ServiceProvider.GetRequiredService<ConnectionStringBuilder>().BuildConnectionString();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<MigrationRunnerFactory>().Create(connectionString);

        runner.MigrateUp();

        return Task.CompletedTask;
    }
}