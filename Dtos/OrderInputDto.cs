
using System.Collections.Generic;

namespace Dtos;

public class OrderInputDto
{
     public string CustomerName { get; set; } = "Default Customer"; 
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool Taxable { get; set; }
}
