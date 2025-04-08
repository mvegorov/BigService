namespace Domain.Entities.OrderHistoryItemPayloads;

public class ItemRemovedPayload : IItemPayload
{
    public ItemRemovedPayload(long productId)
    {
        ProductId = productId;
    }

    public long ProductId { get; set; }
}