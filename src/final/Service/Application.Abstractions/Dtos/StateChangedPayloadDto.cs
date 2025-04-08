using Domain.Entities;

namespace Application.Abstractions.Dtos;

public record StateChangedPayloadDto : ItemPayloadDto
{
    public StateChangedPayloadDto(OrderState oldStatus, OrderState newStatus)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    public OrderState OldStatus { get; set; }

    public OrderState NewStatus { get; set; }
}