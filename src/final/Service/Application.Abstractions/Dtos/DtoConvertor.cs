using Domain.Entities;
using Domain.Entities.OrderHistoryItemPayloads;

namespace Application.Abstractions.Dtos;

public static class DtoConvertor
{
    public static OrderHistoryItemDto ToDto(this OrderHistoryItem orderHistoryItem)
    {
        return new OrderHistoryItemDto(
            orderHistoryItem.OrderId,
            orderHistoryItem.ChangeDate,
            orderHistoryItem.Payload.ToDto());
    }

    public static ItemPayloadDto ToDto(this IItemPayload itemPayload) => itemPayload switch
    {
        ItemAddedPayload addedPayload => new ItemAddedPayloadDto(addedPayload.ProductId, addedPayload.Quantity),
        ItemRemovedPayload removedPayload => new ItemRemovedPayloadDto(removedPayload.ProductId),
        OrderCreatedPayload createdPayload => new OrderCreatedPayloadDto(createdPayload.CreatedBy),
        StateChangedPayload stateChangedPayload => new StateChangedPayloadDto(stateChangedPayload.NewStatus, stateChangedPayload.OldStatus),
        _ => throw new InvalidOperationException($"Unknown payload type: {itemPayload.GetType().Name}"),
    };
}