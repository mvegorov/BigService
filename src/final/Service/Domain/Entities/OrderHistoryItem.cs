using Domain.Entities.OrderHistoryItemPayloads;

namespace Domain.Entities;

public class OrderHistoryItem
{
    public OrderHistoryItem(long orderId, DateTime changeDate, OrderHistoryItemKind itemKind, IItemPayload payload)
    {
        OrderId = orderId;
        ChangeDate = changeDate;
        ItemKind = itemKind;
        Payload = payload;
    }

    public OrderHistoryItem(long? id, long orderId, DateTime changeDate, OrderHistoryItemKind itemKind, IItemPayload payload)
    {
        Id = id;
        OrderId = orderId;
        ChangeDate = changeDate;
        ItemKind = itemKind;
        Payload = payload;
    }

    public long? Id { get; set; }

    public long OrderId { get; set; }

    public DateTime ChangeDate { get; set; }

    public OrderHistoryItemKind ItemKind { get; set; }

    public IItemPayload Payload { get; set; }
}
