#pragma warning disable IDE0005
using Application.Abstractions.Service;
using Application.Ports.Service;
using Domain.Entities;
using Grpc.Core;
using GrpcGeneratedClasses;
using Kafka.Abstractions.Produser;
using Orders.Kafka.Contracts;
using OrderState = Domain.Entities.OrderState;

namespace Presentation.Controllers.Grpc;

public class OrderGrpcController : GrpcGeneratedClasses.Orders.OrdersBase
{
    private readonly IOrderService _orderService;

    private readonly IKafkaProducer<OrderCreationKey, OrderCreationValue> _kafkaProducer;

    public OrderGrpcController(IOrderService orderService, IKafkaProducer<OrderCreationKey, OrderCreationValue> kafkaProducer)
    {
        _orderService = orderService;
        _kafkaProducer = kafkaProducer;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var order = new Order(request.CreatedBy);
        await _orderService.CreateOrderAsync(order, context.CancellationToken);

        if (order.Id is null)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }

        var key = new OrderCreationKey { OrderId = order.Id.Value };
        var value = new OrderCreationValue
        {
            OrderCreated = new OrderCreationValue.Types.OrderCreated
            {
                OrderId = order.Id.Value,
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
            },
        };

        await _kafkaProducer.ProduceAsync(key, value, context.CancellationToken);

        return new CreateOrderResponse { Id = order.Id.Value };
    }

    public override async Task<AddProductToOrderResponse> AddProductToOrder(AddProductToOrderRequest request, ServerCallContext context)
    {
        var item = new OrderItem(request.OrderId, request.ProductId, request.Quantity);
        await _orderService.AddProductToOrderAsync(item, context.CancellationToken);
        return new AddProductToOrderResponse { Message = "Product added to order." };
    }

    public override async Task<RemoveProductFromOrderResponse> RemoveProductFromOrder(RemoveProductFromOrderRequest request, ServerCallContext context)
    {
        await _orderService.RemoveProductFromOrderAsync(request.OrderId, request.ProductId, context.CancellationToken);
        return new RemoveProductFromOrderResponse { Message = "Product deleted from order." };
    }

    public override async Task<UpdateOrderStateResponse> UpdateOrderState(UpdateOrderStateRequest request, ServerCallContext context)
    {
        await _orderService.UpdateOrderStateAsync(request.OrderId, request.NewState.FromRpc(), context.CancellationToken);

        if (request.NewState.FromRpc() == OrderState.Processing)
        {
            var key = new OrderCreationKey { OrderId = request.OrderId };
            var value = new OrderCreationValue
            {
                OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
                {
                    OrderId = request.OrderId,
                    StartedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                },
            };

            await _kafkaProducer.ProduceAsync(key, value, context.CancellationToken);
        }

        return new UpdateOrderStateResponse { Message = "Order state updated." };
    }

    public override async Task GetOrderHistory(GetOrderHistoryRequest request, IServerStreamWriter<OrderHistoryEntry> responseStream, ServerCallContext context)
    {
        List<OrderHistoryItem> historyItems = await _orderService.GetOrderHistoryAsync(request.OrderId, request.Cursor, request.PageSize, context.CancellationToken)
            .ToListAsync(context.CancellationToken);

        foreach (OrderHistoryItem item in historyItems)
        {
            await responseStream.WriteAsync(item.ToRpc());
        }
    }

    public override async Task GetFullOrderHistory(GetFullOrderHistoryRequest request, IServerStreamWriter<OrderHistoryEntry> responseStream, ServerCallContext context)
    {
        List<OrderHistoryItem> historyItems = await _orderService.GetFullOrderHistoryAsync(request.OrderId, context.CancellationToken)
            .ToListAsync(context.CancellationToken);

        foreach (OrderHistoryItem item in historyItems)
        {
            await responseStream.WriteAsync(item.ToRpc());
        }
    }
}