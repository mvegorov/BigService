namespace Domain.Entities.OrderHistoryItemPayloads;

public class OrderCreatedPayload : IItemPayload
{
    public OrderCreatedPayload(string createdBy)
    {
        CreatedBy = createdBy;
    }

    public string CreatedBy { get; set; }
}