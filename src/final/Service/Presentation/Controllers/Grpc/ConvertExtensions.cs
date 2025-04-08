using Domain.Entities;
using Domain.Entities.OrderHistoryItemPayloads;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGeneratedClasses;
using ItemAddedPayload = GrpcGeneratedClasses.ItemAddedPayload;

namespace Presentation.Controllers.Grpc;

public static class ConvertExtensions
{
    public static decimal ToDecimal(this Google.Type.Money money)
    {
        ArgumentNullException.ThrowIfNull(money);

        if (money.CurrencyCode != "USD")
        {
            throw new RpcException(new Status(StatusCode.Unavailable, "USD is the only supported currency"));
        }

        decimal dollars = money.Units;
        decimal cents = money.Nanos / 10_000_000_000m;

        return dollars + cents;
    }

    public static Domain.Entities.OrderState FromRpc(this GrpcGeneratedClasses.OrderState state) => state switch
    {
        GrpcGeneratedClasses.OrderState.Created => Domain.Entities.OrderState.Created,
        GrpcGeneratedClasses.OrderState.Processing => Domain.Entities.OrderState.Processing,
        GrpcGeneratedClasses.OrderState.Cancelled => Domain.Entities.OrderState.Cancelled,
        GrpcGeneratedClasses.OrderState.Completed => Domain.Entities.OrderState.Completed,
        GrpcGeneratedClasses.OrderState.Unknown => throw new RpcException(new Status(StatusCode.Unimplemented, "Unimplemented OrderState")),
        _ => throw new RpcException(new Status(StatusCode.Unavailable, "Unknown order state")),
    };

    public static GrpcGeneratedClasses.OrderState ToRpc(this Domain.Entities.OrderState state) => state switch
    {
        Domain.Entities.OrderState.Created => GrpcGeneratedClasses.OrderState.Created,
        Domain.Entities.OrderState.Processing => GrpcGeneratedClasses.OrderState.Processing,
        Domain.Entities.OrderState.Cancelled => GrpcGeneratedClasses.OrderState.Cancelled,
        Domain.Entities.OrderState.Completed => GrpcGeneratedClasses.OrderState.Completed,
        _ => GrpcGeneratedClasses.OrderState.Unknown,
    };

    public static OrderHistoryEntry ToRpc(this OrderHistoryItem item)
    {
        return new OrderHistoryEntry
        {
            Id = item.Id ?? throw new InvalidCastException("Id cannot be null."),
            OrderId = item.OrderId,
            ChangeDate = Timestamp.FromDateTime(item.ChangeDate),
            Payload = item.Payload.ToRpc(),
        };
    }

    public static GrpcGeneratedClasses.Payload ToRpc(this IItemPayload payload) => payload switch
    {
        Domain.Entities.OrderHistoryItemPayloads.ItemAddedPayload added => new GrpcGeneratedClasses.Payload
        {
            ItemAdded = new ItemAddedPayload { ProductId = added.ProductId, Quantity = added.Quantity },
        },

        Domain.Entities.OrderHistoryItemPayloads.ItemRemovedPayload removed => new GrpcGeneratedClasses.Payload
        {
            ItemRemoved = new GrpcGeneratedClasses.ItemRemovedPayload { ProductId = removed.ProductId, },
        },

        Domain.Entities.OrderHistoryItemPayloads.OrderCreatedPayload created => new GrpcGeneratedClasses.Payload
        {
            OrderCreated = new GrpcGeneratedClasses.OrderCreatedPayload { CreatedBy = created.CreatedBy, },
        },

        Domain.Entities.OrderHistoryItemPayloads.StateChangedPayload changed => new GrpcGeneratedClasses.Payload
        {
            StateChanged = new GrpcGeneratedClasses.StateChangedPayload { OldStatus = changed.OldStatus.ToRpc(), NewStatus = changed.NewStatus.ToRpc(), },
        },

        _ => throw new RpcException(new Status(StatusCode.Unavailable, "Unknown order state")),
    };
}