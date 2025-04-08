using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Product
{
    public long? Id { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    [JsonConstructor]
    public Product(long id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}