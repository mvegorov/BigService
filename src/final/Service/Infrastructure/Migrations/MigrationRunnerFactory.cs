using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Migrations;

public class MigrationRunnerFactory
{
    public IMigrationRunner Create(string connectionString)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddMigrations(connectionString)
            .BuildServiceProvider(false);

        return serviceProvider.GetRequiredService<IMigrationRunner>();
    }
}