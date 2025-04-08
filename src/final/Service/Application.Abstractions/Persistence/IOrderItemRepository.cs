using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;

namespace Application.Abstractions.Persistence;

public interface IOrderItemRepository
{
    // Добавление позиции заказа
    public Task<long> InsertOrderItemAsync(
        OrderItem item,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    public Task DeleteOrderItemAsync(
        long orderItemId,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    public Task DeleteOrderItemAsync(
        long orderId,
        long productId,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    // Пагинированный поиск по позициям заказов
    public IAsyncEnumerable<OrderItem> SearchOrderItemsAsync(
        OrderItemQuery query,
        CancellationToken cancellationToken);
}