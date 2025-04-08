namespace Domain.Entities;

public class OrderItem
{
    public OrderItem(long? id, long orderId, long productId, int quantity, bool isDeleted)
    {
        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        IsDeleted = isDeleted;
    }

    public OrderItem(long orderId, long productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        IsDeleted = false;
    }

    public long? Id { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int Quantity { get; set; }

    public bool IsDeleted { get; set; }
}