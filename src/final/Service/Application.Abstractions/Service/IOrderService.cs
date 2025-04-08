using Domain.Entities;

namespace Application.Abstractions.Service;

public interface IOrderService
{
    public Task CreateOrderAsync(Order order, CancellationToken cancellationToken);

    public Task AddProductToOrderAsync(OrderItem item, CancellationToken cancellationToken);

    public Task RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken);

    public Task UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken);

    public IAsyncEnumerable<OrderHistoryItem> GetOrderHistoryAsync(
        long orderId,
        int cursor,
        int pageSize,
        CancellationToken cancellationToken);

    public IAsyncEnumerable<OrderHistoryItem> GetFullOrderHistoryAsync(
        long orderId,
        CancellationToken cancellationToken);
}