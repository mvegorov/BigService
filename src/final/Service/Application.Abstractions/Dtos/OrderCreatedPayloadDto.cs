namespace Application.Abstractions.Dtos;

public record OrderCreatedPayloadDto : ItemPayloadDto
{
    public OrderCreatedPayloadDto(string createdBy)
    {
        CreatedBy = createdBy;
    }

    public string CreatedBy { get; set; }
}