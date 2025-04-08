using Application.Abstractions.Persistence;
using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly INpgsqlConnectionProvider _connectionProvider;

    public OrderRepository(INpgsqlConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    // Создание заказа
    public async Task<long> InsertOrderAsync(Order order, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = """
                           insert into orders (order_created_at, order_state, order_created_by) 
                           values (@date, @state, @creator)
                           RETURNING order_id;
                           """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@date", order.Date),
                new NpgsqlParameter("@state", order.State),
                new NpgsqlParameter("@creator", order.Creator),
            },
        };

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new InvalidOperationException("Failed to insert order.");

        return Convert.ToInt64(result);
    }

    // Изменение статуса заказа
    public async Task UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = "update orders set order_state = @state where order_id = @id;";

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@id", orderId),
                new NpgsqlParameter("@state", newState),
            },
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Пагинированный поиск заказов
    public async IAsyncEnumerable<Order> SearchOrdersAsync(
        OrderQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        const string sqlQuery = """
                                select * from orders
                                where
                                  (order_id > @cursor)
                                  and (cardinality(@ids) = 0 or order_id = any (@ids))
                                  and (cardinality(@states) = 0 or order_state = any (@states))
                                  and (@creator = null or order_created_by = @creator)
                                order by order_id
                                limit @page_size;
                                """;

        using var command = new NpgsqlCommand(sqlQuery, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("@ids", query.Ids),
                new NpgsqlParameter("@states", query.States),
                new NpgsqlParameter("@cursor", query.Cursor),
                new NpgsqlParameter("@page_size", query.PageSize),
                new NpgsqlParameter("@creator", query.Creator ?? (object)DBNull.Value),
            },
        };

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Order(
                reader.GetInt64(reader.GetOrdinal("order_id")),
                reader.GetDateTime(reader.GetOrdinal("order_created_at")),
                Enum.Parse<OrderState>(reader.GetString(reader.GetOrdinal("order_state"))),
                reader.GetString(reader.GetOrdinal("order_created_by")));
        }
    }

    public async Task<Order> FindOrderById(long orderId, CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        const string sqlQuery = """
                                select * from orders
                                where (order_id = @id);
                                """;

        using var command = new NpgsqlCommand(sqlQuery, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("@id", orderId),
            },
        };

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Order(
            reader.GetInt64(reader.GetOrdinal("order_id")),
            reader.GetDateTime(reader.GetOrdinal("order_created_at")),
            Enum.Parse<OrderState>(reader.GetString(reader.GetOrdinal("order_state")), ignoreCase: true),
            reader.GetString(reader.GetOrdinal("order_created_by")));
    }
}
