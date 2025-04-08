namespace Application.Abstractions.Dtos;

public record OrderHistoryItemDto
{
    public OrderHistoryItemDto(long orderId, DateTime changeDate, ItemPayloadDto payload)
    {
        OrderId = orderId;
        ChangeDate = changeDate;
        Payload = payload;
    }

    public long? Id { get; set; }

    public long OrderId { get; set; }

    public DateTime ChangeDate { get; set; }

    public ItemPayloadDto Payload { get; set; }
}