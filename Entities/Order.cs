
using System.Collections.Generic;

namespace Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public Payment Payment { get; set; } = null!;
    public ShippingDetail ShippingDetail { get; set; } = null!;
}
