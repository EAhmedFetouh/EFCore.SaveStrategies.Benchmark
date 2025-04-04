
using System.Collections.Generic;

namespace Dtos;

public static class OrderDataGenerator
{
    public static List<OrderInputDto> Generate(int count)
    {
        var result = new List<OrderInputDto>();
        for (int i = 0; i < count; i++)
        {
            result.Add(new OrderInputDto
            {
                Items = new List<OrderItemDto>
                {
                    new() { Quantity = 2, UnitPrice = 100, Taxable = true },
                    new() { Quantity = 3, UnitPrice = 50, Taxable = false },
                    new() { Quantity = 0, UnitPrice = 20, Taxable = true },
                }
            });
        }
        return result;
    }
}
