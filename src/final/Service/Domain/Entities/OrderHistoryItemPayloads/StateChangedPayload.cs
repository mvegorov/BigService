namespace Domain.Entities.OrderHistoryItemPayloads;

public class StateChangedPayload : IItemPayload
{
    public StateChangedPayload(OrderState oldStatus, OrderState newStatus)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public OrderState OldStatus { get; set; }

    public OrderState NewStatus { get; set; }
}