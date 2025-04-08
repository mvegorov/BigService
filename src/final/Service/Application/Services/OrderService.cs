using Application.Abstractions.Persistence;
using Application.Abstractions.Queries;
using Application.Abstractions.Service;
using Domain.Entities;
using Domain.Entities.OrderHistoryItemPayloads;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly INpgsqlConnectionProvider _connectionProvider;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderHistoryRepository orderHistoryRepository,
        IOrderItemRepository orderItemRepository,
        INpgsqlConnectionProvider connectionProvider)
    {
        _orderRepository = orderRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _orderItemRepository = orderItemRepository;
        _connectionProvider = connectionProvider;
    }

    public async Task CreateOrderAsync(Order order, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            long generatedId = await _orderRepository.InsertOrderAsync(order, cancellationToken, transaction);
            order.Id = generatedId;
            await _orderHistoryRepository.InsertOrderHistoryItemAsync(
                new OrderHistoryItem(
                    order.Id.Value,
                    DateTime.Now,
                    OrderHistoryItemKind.Created,
                    new OrderCreatedPayload(order.Creator)),
                cancellationToken)
                ;

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task AddProductToOrderAsync(OrderItem item, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.FindOrderById(item.OrderId, cancellationToken);

        if (order == null || order.Id == null)
        {
            throw new NullReferenceException("Order not found.");
        }

        if (order.State != OrderState.Created)
            throw new InvalidOperationException("Products can't be added to order.");

        await using NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            long generatedId = await _orderItemRepository.InsertOrderItemAsync(item, cancellationToken);
            item.Id = generatedId;
            await _orderHistoryRepository.InsertOrderHistoryItemAsync(
                new OrderHistoryItem(
                    order.Id.Value,
                    DateTime.Now,
                    OrderHistoryItemKind.ItemAdded,
                    new ItemAddedPayload(item.Id.Value, item.Quantity)),
                cancellationToken)
                ;

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.FindOrderById(orderId, cancellationToken);

        if (order == null || order.Id == null)
        {
            throw new NullReferenceException("Order not found.");
        }

        if (order.State != OrderState.Created)
            throw new InvalidOperationException("Products can only be removed from orders with status 'created'.");

        await using NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderItemRepository.DeleteOrderItemAsync(orderId, productId, cancellationToken);
            await _orderHistoryRepository.InsertOrderHistoryItemAsync(
                    new OrderHistoryItem(
                        order.Id.Value,
                        DateTime.Now,
                        OrderHistoryItemKind.ItemRemoved,
                        new ItemRemovedPayload(orderId)),
                    cancellationToken)
                ;

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken)
    {
        Order? order = await _orderRepository.FindOrderById(orderId, cancellationToken);

        if (order == null || order.Id == null)
        {
            throw new NullReferenceException("Order not found.");
        }

        if (newState == OrderState.Processing && order.State != OrderState.Created)
            throw new InvalidOperationException("Only orders in 'created' status can be moved to 'processing'.");

        if (newState == OrderState.Completed && order.State != OrderState.Processing)
            throw new InvalidOperationException("Only orders in 'processing' status can be completed.");

        if (newState == OrderState.Cancelled && order.State == OrderState.Completed)
            throw new InvalidOperationException("Completed orders cannot be cancelled.");

        OrderState oldState = order.State;
        order.State = newState;

        await using NpgsqlConnection connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderRepository.UpdateOrderStateAsync(orderId, newState, cancellationToken);
            await _orderHistoryRepository.InsertOrderHistoryItemAsync(
                    new OrderHistoryItem(
                        order.Id.Value,
                        DateTime.Now,
                        OrderHistoryItemKind.StateChanged,
                        new StateChangedPayload(oldState, newState)),
                    cancellationToken)
                ;

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async IAsyncEnumerable<OrderHistoryItem> GetOrderHistoryAsync(long orderId, int cursor, int pageSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = new OrderHistoryQuery(new List<long> { orderId }, new List<OrderHistoryItemKind> { }, cursor, pageSize);
        await foreach (OrderHistoryItem history in _orderHistoryRepository.SearchOrderHistoryAsync(query, cancellationToken))
        {
            yield return history;
        }
    }

    public async IAsyncEnumerable<OrderHistoryItem> GetFullOrderHistoryAsync(long orderId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        int pageSize = 10;
        long cursor = 0;

        int count = pageSize;
        while (count == pageSize)
        {
            count = 0;
            var query = new OrderHistoryQuery(new List<long> { orderId }, new List<OrderHistoryItemKind> { }, cursor, pageSize);
            await foreach (OrderHistoryItem historyItem in _orderHistoryRepository.SearchOrderHistoryAsync(query, cancellationToken))
            {
                if (historyItem.Id == null)
                {
                    yield break;
                }

                count += 1;
                if (count == pageSize)
                {
                    cursor = historyItem.Id.Value;
                }

                yield return historyItem;
            }
        }
    }
}