using Application.Abstractions.Persistence;
using Domain.Entities;
using Npgsql;

namespace Infrastructure.Repositories.Connection;

public class ConnectionProvider : INpgsqlConnectionProvider
{
    private readonly ConnectionStringBuilder _connectionStringBuilder;

    public ConnectionProvider(ConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
    }

    public async ValueTask<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        string connectionString = _connectionStringBuilder.BuildConnectionString();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<OrderHistoryItemKind>("order_history_item_kind");
        dataSourceBuilder.MapEnum<OrderState>("order_state");

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();
        NpgsqlConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);

        return connection;
    }
}