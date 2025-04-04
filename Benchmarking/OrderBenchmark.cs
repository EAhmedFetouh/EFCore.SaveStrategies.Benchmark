
using BenchmarkDotNet.Attributes;
using Methods;
using Dtos;
using System.Collections.Generic;
using Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Benchmarking;

[MemoryDiagnoser]
public class OrderBenchmark
{
    private readonly OrderService _service;
    private List<OrderInputDto> _orders;

    public OrderBenchmark()
    {
        var factory = new PooledDbContextFactory<AppDbContext>(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=performance;Trusted_Connection=True;Encrypt=False")
                .Options
        );

        _service = new OrderService(factory); 
        _orders = OrderDataGenerator.Generate(500);
    }

    [Benchmark]
    public async Task SaveWithForeachAndIf() => await _service.SaveWithForeachAndIf(_orders);

    [Benchmark]
    public async Task SaveWithLinq() => await _service.SaveWithLinq(_orders);

    [Benchmark]
    public async Task SaveWithAddRange() => await _service.SaveWithAddRange(_orders);

    [Benchmark]
    public async Task SaveParallelSafe() => await _service.SaveParallelSafe(_orders);

    [Benchmark]
    public async Task SaveParallelImproved() => await _service.SaveParallelImproved(_orders);

    [Benchmark]
    public async Task SaveParallelBatches() => await _service.SaveParallelBatches(_orders);
}
