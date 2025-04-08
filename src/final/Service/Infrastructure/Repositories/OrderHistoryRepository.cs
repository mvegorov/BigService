using Application.Abstractions.Persistence;
using Application.Abstractions.Queries;
using Domain.Entities;
using Domain.Entities.OrderHistoryItemPayloads;
using Npgsql;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Infrastructure.Repositories;

public class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly INpgsqlConnectionProvider _connectionProvider;

    public OrderHistoryRepository(INpgsqlConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    // Метод для добавления записи в историю заказа
    public async Task<long> InsertOrderHistoryItemAsync(OrderHistoryItem item, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null)
    {
        const string sql = """
                           insert into order_history (order_id, order_history_item_kind, order_history_item_created_at, order_history_item_payload)
                           values (@orderId, @type, @date, @data)
                           RETURNING order_history_item_id;
                           """;

        string jsonData = JsonSerializer.Serialize(item.Payload);

        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            Parameters =
            {
                new NpgsqlParameter("@orderId", item.OrderId),
                new NpgsqlParameter("@type", item.ItemKind),
                new NpgsqlParameter("@date", item.ChangeDate),
                new NpgsqlParameter("@data", jsonData) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb },
            },
        };

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new InvalidOperationException("Failed to insert order history item.");

        return Convert.ToInt64(result);
    }

    // Метод для пагинированного поиска по истории заказов
    public async IAsyncEnumerable<OrderHistoryItem> SearchOrderHistoryAsync(
        OrderHistoryQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        const string sqlQuery = """
                                select * from order_history 
                                         where 
                                           (order_history_item_id > @cursor)
                                           and (cardinality(@order_ids) = 0 or order_id = any (@order_ids)) 
                                           and (cardinality(@kinds) = 0 or order_history_item_kind = any (@kinds))
                                         order by order_history_item_id 
                                         limit @page_size;
                                """;

        using var command = new NpgsqlCommand(sqlQuery, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("@order_ids", query.OrderIds),
                new NpgsqlParameter("@kinds", query.ItemKinds),
                new NpgsqlParameter("@cursor", query.Cursor),
                new NpgsqlParameter("@page_size", query.PageSize),
            },
        };

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            IItemPayload? payload =
                JsonSerializer.Deserialize<IItemPayload>(
                    reader.GetString(reader.GetOrdinal("order_history_item_payload")));
            if (payload == null)
            {
                throw new NullReferenceException();
            }

            yield return new OrderHistoryItem(
                reader.GetInt64(reader.GetOrdinal("order_history_item_id")),
                reader.GetInt64(reader.GetOrdinal("order_id")),
                reader.GetDateTime(reader.GetOrdinal("order_history_item_created_at")),
                Enum.Parse<OrderHistoryItemKind>(reader.GetString(reader.GetOrdinal("order_history_item_kind")).Replace("_", string.Empty, StringComparison.Ordinal), ignoreCase: true),
                payload);
        }
    }
}
