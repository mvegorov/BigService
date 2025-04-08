namespace Domain.Entities;

public class Order
{
    public Order(long? id, DateTime date, OrderState state, string creator)
    {
        Id = id;
        Date = date;
        State = state;
        Creator = creator;
    }

    public Order(string creator)
    {
        Date = DateTime.Now;
        State = OrderState.Created;
        Creator = creator;
    }

    public long? Id { get; set; }

    public DateTime Date { get; set; }

    public OrderState State { get; set; }

    public string Creator { get; set; }
}