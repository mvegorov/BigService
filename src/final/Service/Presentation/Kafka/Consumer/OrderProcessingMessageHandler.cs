#pragma warning disable IDE0005
using Application.Abstractions.Service;
using Confluent.Kafka;
using Domain.Entities;
using Kafka.Abstractions.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Orders.Kafka.Contracts;

namespace Presentation.Kafka.Consumer;

public class OrderProcessingMessageHandler : IKafkaMessageHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IOrderService _orderService;

    public OrderProcessingMessageHandler(IOrderService orderService, IServiceScopeFactory scopeFactory)
    {
        _orderService = orderService;
    }

    public async Task HandleAsync(IReadOnlyCollection<ConsumeResult<OrderProcessingKey, OrderProcessingValue>> messages, CancellationToken cancellationToken)
    {
        foreach (ConsumeResult<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            long orderId = message.Message.Key.OrderId;
            OrderProcessingValue val = message.Message.Value;

            switch (val.EventCase)
            {
                case OrderProcessingValue.EventOneofCase.ApprovalReceived:
                    OrderProcessingValue.Types.OrderApprovalReceived approval = val.ApprovalReceived;
                    if (!approval.IsApproved)
                    {
                        await _orderService.UpdateOrderStateAsync(approval.OrderId, OrderState.Cancelled, cancellationToken);
                    }

                    break;

                case OrderProcessingValue.EventOneofCase.PackingFinished:
                    OrderProcessingValue.Types.OrderPackingFinished packingFinished = val.PackingFinished;
                    if (!packingFinished.IsFinishedSuccessfully)
                    {
                        await _orderService.UpdateOrderStateAsync(packingFinished.OrderId, OrderState.Cancelled, cancellationToken);
                    }

                    break;

                case OrderProcessingValue.EventOneofCase.DeliveryFinished:
                    OrderProcessingValue.Types.OrderDeliveryFinished deliveryFinished = val.DeliveryFinished;
                    if (!deliveryFinished.IsFinishedSuccessfully)
                    {
                        await _orderService.UpdateOrderStateAsync(deliveryFinished.OrderId, OrderState.Cancelled, cancellationToken);
                    }
                    else
                    {
                        await _orderService.UpdateOrderStateAsync(deliveryFinished.OrderId, OrderState.Completed, cancellationToken);
                    }

                    break;
            }
        }
    }
}