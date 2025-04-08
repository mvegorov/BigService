using Application.Abstractions.Persistence;
using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly INpgsqlConnectionProvider _connectionProvider;

    public OrderItemRepository(INpgsqlConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    // Добавление позиции заказа
    public async Task<long> InsertOrderItemAsync(OrderItem item, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = """
                           insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted) 
                           values (@orderId, @productId, @quantity, false)
                           RETURNING order_item_id;
                           """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@orderId", item.OrderId),
                new NpgsqlParameter("@productId", item.ProductId),
                new NpgsqlParameter("@quantity", item.Quantity),
            },
        };

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new InvalidOperationException("Failed to insert order item.");

        return Convert.ToInt64(result);
    }

    public async Task DeleteOrderItemAsync(long orderItemId, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = "update order_items set deleted = true where order_item_id = @id;";

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters = { new NpgsqlParameter("@id", orderItemId) },
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteOrderItemAsync(long orderId, long productId, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = """
                           update order_items set order_item_deleted = true
                           where 
                               (order_id = @orderId)
                               and (product_id = @productId);
                           """;

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@orderId", orderId),
                new NpgsqlParameter("@productId", productId),
            },
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    // Пагинированный поиск по позициям заказов
    public async IAsyncEnumerable<OrderItem> SearchOrderItemsAsync(
        OrderItemQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        const string sqlQuery = """
                                select * from order_items
                                where
                                  (order_id > @cursor)
                                  and (order_id = any (@orderIds))
                                  and (product_id = any (@productIds))
                                  and (@deleted is null or order_item_deleted = @deleted)
                                order by order_item_id
                                limit @page_size;
                                """;

        using var command = new NpgsqlCommand(sqlQuery, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("@orderIds", query.OrderIds),
                new NpgsqlParameter("@productIds", query.ProductIds),
                new NpgsqlParameter("@deleted", query.IsDeleted),
                new NpgsqlParameter("@cursor", query.Cursor),
                new NpgsqlParameter("@page_size", query.PageSize),
            },
        };

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderItem(
                reader.GetInt64(reader.GetOrdinal("order_item_id")),
                reader.GetInt64(reader.GetOrdinal("order_id")),
                reader.GetInt64(reader.GetOrdinal("product_id")),
                reader.GetInt32(reader.GetOrdinal("quantity")),
                reader.GetBoolean(reader.GetOrdinal("deleted")));
        }
    }
}
