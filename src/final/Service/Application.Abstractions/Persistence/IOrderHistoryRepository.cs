using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;

namespace Application.Abstractions.Persistence;

public interface IOrderHistoryRepository
{
    // Метод для добавления записи в историю заказа
    public Task<long> InsertOrderHistoryItemAsync(OrderHistoryItem item, CancellationToken cancellationToken, NpgsqlTransaction? transaction = null);

    // Метод для пагинированного поиска по истории заказов
    public IAsyncEnumerable<OrderHistoryItem> SearchOrderHistoryAsync(OrderHistoryQuery query, CancellationToken cancellationToken);
}