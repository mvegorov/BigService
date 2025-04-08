namespace Application.Abstractions.Dtos;

public record ItemAddedPayloadDto : ItemPayloadDto
{
    public ItemAddedPayloadDto(long productId, int quantity)
    {
        ProductId = productId;

        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        Quantity = quantity;
    }

    public long ProductId { get; set; }

    public int Quantity { get; set; }
}