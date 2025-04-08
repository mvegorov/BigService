namespace Application.Abstractions.Dtos;

public record ItemRemovedPayloadDto : ItemPayloadDto
{
    public ItemRemovedPayloadDto(long productId)
    {
        ProductId = productId;
    }

    public long ProductId { get; set; }
}