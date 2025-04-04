
using System.Collections.Generic;

namespace Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsTaxable { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
