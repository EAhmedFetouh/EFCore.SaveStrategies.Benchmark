
using System;

namespace Entities;

public class ShippingDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
}
