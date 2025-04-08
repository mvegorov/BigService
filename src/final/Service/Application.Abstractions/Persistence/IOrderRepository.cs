using Application.Abstractions.Queries;
using Domain.Entities;
using Npgsql;

namespace Application.Abstractions.Persistence;

public interface IOrderRepository
{
    public Task<long> InsertOrderAsync(
        Order order,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    // Изменение статуса заказа
    public Task UpdateOrderStateAsync(
        long orderId,
        OrderState newState,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null);

    // Пагинированный поиск заказов
    public IAsyncEnumerable<Order> SearchOrdersAsync(
        OrderQuery query,
        CancellationToken cancellationToken);

    public Task<Order> FindOrderById(long orderId, CancellationToken cancellationToken);
}