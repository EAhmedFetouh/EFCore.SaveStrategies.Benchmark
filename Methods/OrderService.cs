
using Dtos;
using Entities;
using Microsoft.EntityFrameworkCore;
using Database;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Methods;

public class OrderService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public OrderService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // 1. Add + foreach + if
    public async Task SaveWithForeachAndIf(List<OrderInputDto> orders)
    {
        await using var db = _contextFactory.CreateDbContext();

        foreach (var dto in orders)
        {
            if (dto.CustomerName != null && dto.Items.Any())
            {
                var customer = new Customer { Name = dto.CustomerName };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();

                var order = new Order { CustomerId = customer.Id };
                db.Orders.Add(order);
                await db.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    if (item.Quantity > 0 && item.UnitPrice > 0)
                    {
                        db.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Taxable = item.Taxable
                        });
                    }
                }

                db.Payments.Add(new Payment { OrderId = order.Id, Amount = 100, PaidAt = DateTime.UtcNow });
                db.ShippingDetails.Add(new ShippingDetail { OrderId = order.Id, Address = "Test", ShippedAt = DateTime.UtcNow });

                await db.SaveChangesAsync();
            }
        }
    }

    // 2. Add + LINQ
    public async Task SaveWithLinq(List<OrderInputDto> orders)
    {
        await using var db = _contextFactory.CreateDbContext();

        foreach (var dto in orders.Where(o => o.CustomerName != null && o.Items.Any()))
        {
            var customer = new Customer { Name = dto.CustomerName };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            var order = new Order { CustomerId = customer.Id };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var items = dto.Items
                .Where(i => i.Quantity > 0 && i.UnitPrice > 0)
                .Select(i => new OrderItem
                {
                    OrderId = order.Id,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Taxable = i.Taxable
                })
                .ToList();

            db.OrderItems.AddRange(items);
            db.Payments.Add(new Payment { OrderId = order.Id, Amount = 100, PaidAt = DateTime.UtcNow });
            db.ShippingDetails.Add(new ShippingDetail { OrderId = order.Id, Address = "Test", ShippedAt = DateTime.UtcNow });

            await db.SaveChangesAsync();
        }
    }

    // 3. AddRange batching
    public async Task SaveWithAddRange(List<OrderInputDto> orders)
    {
        await using var db = _contextFactory.CreateDbContext();

        var customers = orders.Select(o => new Customer { Name = o.CustomerName }).ToList();
        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();

        var orderEntities = customers.Select(c => new Order { CustomerId = c.Id }).ToList();
        db.Orders.AddRange(orderEntities);
        await db.SaveChangesAsync();

        var allItems = new List<OrderItem>();
        var allPayments = new List<Payment>();
        var allShipping = new List<ShippingDetail>();

        for (int i = 0; i < orders.Count; i++)
        {
            var order = orderEntities[i];
            var dto = orders[i];

            allItems.AddRange(dto.Items.Select(item => new OrderItem
            {
                OrderId = order.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Taxable = item.Taxable
            }));

            allPayments.Add(new Payment { OrderId = order.Id, Amount = 100, PaidAt = DateTime.UtcNow });
            allShipping.Add(new ShippingDetail { OrderId = order.Id, Address = "Test", ShippedAt = DateTime.UtcNow });
        }

        db.OrderItems.AddRange(allItems);
        db.Payments.AddRange(allPayments);
        db.ShippingDetails.AddRange(allShipping);

        await db.SaveChangesAsync();
    }

    // 4. Parallel with DbContextFactory (safe)
    public async Task SaveParallelSafe(List<OrderInputDto> orders)
    {
        var tasks = orders.Select(async dto =>
        {
            await using var db = _contextFactory.CreateDbContext();

            var customer = new Customer { Name = dto.CustomerName };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            var order = new Order { CustomerId = customer.Id };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            db.OrderItems.AddRange(dto.Items.Select(item => new OrderItem
            {
                OrderId = order.Id,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Taxable = item.Taxable
            }));

            db.Payments.Add(new Payment { OrderId = order.Id, Amount = 100, PaidAt = DateTime.UtcNow });
            db.ShippingDetails.Add(new ShippingDetail { OrderId = order.Id, Address = "Test", ShippedAt = DateTime.UtcNow });

            await db.SaveChangesAsync();
        });

        await Task.WhenAll(tasks);
    }

    public async Task SaveParallelImproved(List<OrderInputDto> orders)
    {
        var tasks = orders.Select(async dto =>
        {
            await using var db = _contextFactory.CreateDbContext();

            var customer = new Customer { Name = dto.CustomerName };
            var order = new Order { Customer = customer };

            var items = dto.Items.Select(item => new OrderItem
            {
                Order = order,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Taxable = item.Taxable
            }).ToList();

            var payment = new Payment
            {
                Order = order,
                Amount = 100,
                PaidAt = DateTime.UtcNow
            };

            var shipping = new ShippingDetail
            {
                Order = order,
                Address = "Test",
                ShippedAt = DateTime.UtcNow
            };

            db.Orders.Add(order); 
            db.OrderItems.AddRange(items);
            db.Payments.Add(payment);
            db.ShippingDetails.Add(shipping);

            await db.SaveChangesAsync(); 
        });

        await Task.WhenAll(tasks);
    }

    public async Task SaveParallelBatches(List<OrderInputDto> orders, int batchSize = 100)
    {
        var batches = orders
            .Select((order, index) => new { order, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.order).ToList())
            .ToList();

        var tasks = batches.Select(async batch =>
        {
            await using var db = _contextFactory.CreateDbContext();

            // Add Customers
            var customers = batch.Select(o => new Customer { Name = o.CustomerName }).ToList();
            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();

            // Add Orders
            var orderEntities = customers.Select(c => new Order { CustomerId = c.Id }).ToList();
            db.Orders.AddRange(orderEntities);
            await db.SaveChangesAsync();

            // Build related data with LINQ.Zip
            var items = batch.Zip(orderEntities, (dto, order) =>
                dto.Items.Select(item => new OrderItem
                {
                    OrderId = order.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Taxable = item.Taxable
                })
            ).SelectMany(x => x).ToList();

            var payments = batch.Zip(orderEntities, (dto, order) => new Payment
            {
                OrderId = order.Id,
                Amount = 100,
                PaidAt = DateTime.UtcNow
            }).ToList();

            var shippings = batch.Zip(orderEntities, (dto, order) => new ShippingDetail
            {
                OrderId = order.Id,
                Address = "Test",
                ShippedAt = DateTime.UtcNow
            }).ToList();

            // Add all to context
            db.OrderItems.AddRange(items);
            db.Payments.AddRange(payments);
            db.ShippingDetails.AddRange(shippings);

            await db.SaveChangesAsync();
        });

        await Task.WhenAll(tasks);
    }

}
