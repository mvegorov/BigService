namespace Domain.Entities.OrderHistoryItemPayloads;

public class ItemAddedPayload : IItemPayload
{
    public ItemAddedPayload(long productId, int quantity)
    {
        ProductId = productId;

        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        Quantity = quantity;
    }

    public long ProductId { get; set; }

    public int Quantity { get; set; }
}