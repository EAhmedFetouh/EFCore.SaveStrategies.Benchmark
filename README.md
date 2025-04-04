# SaveStrategies.Benchmark (EF Core)

🎯 **Project Goal**  
This project demonstrates how optimizing your code alone can drastically improve performance when saving data to a database using **Entity Framework Core**—without needing Redis, MongoDB, or any external caching/message systems.

---

## 📊 What Was Measured?

We tested 6 different save strategies, all performing the **same task**:

- Create a `Customer`
- Create an `Order` linked to that customer
- Add `OrderItems`, `Payment`, and `ShippingDetail`

---

## 🧪 Tested Strategies

| Method Name             | Description                                                                 |
|-------------------------|-----------------------------------------------------------------------------|
| `SaveWithForeachAndIf`  | Naive approach with heavy `foreach` and nested `if` conditions              |
| `SaveWithLinq`          | Cleaned up logic using LINQ                                                 |
| `SaveWithAddRange`      | Batch insert using `AddRange`                                               |
| `SaveParallelSafe`      | Parallel processing using `DbContextFactory` + `Task.WhenAll`               |
| `SaveParallelImproved`  | Optimized parallel with fewer `SaveChangesAsync` calls                      |
| `SaveParallelBatches`   | ⚡ Best performer: batching + parallel hybrid approach                       |

---

## 📈 Benchmark Results

| Method                 | Mean Time (ms) | Memory Usage |
|------------------------|----------------|--------------|
| `SaveWithForeachAndIf` | 5005 ms         | 1395 MB 🔥   |
| `SaveWithLinq`         | 4929 ms         | 1395 MB 🔥   |
| `SaveWithAddRange`     | 267 ms          | 35 MB ✅     |
| `SaveParallelSafe`     | 460 ms          | 55 MB ✅     |
| `SaveParallelImproved` | 382 ms          | 55 MB ✅     |
| `SaveParallelBatches`  | **171 ms**      | **35 MB ✅** |

---

## ✅ Conclusion

> Don't reach for Redis, MongoDB, or NoSQL right away when you see a performance issue.

Sometimes:
- Reducing how often you call `SaveChangesAsync`
- Using `AddRange` instead of `Add`
- Or just restructuring the logic with `DbContextFactory` and batching

...can give you **massive** performance gains 💥

---

## 🛠️ Tools Used

- [.NET 8](https://dotnet.microsoft.com/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- SQL Server LocalDB

---

## 🚀 How to Run

```bash
# Apply migrations and update DB
dotnet ef migrations add Init
dotnet ef database update

# Run benchmarks
dotnet run -c Release
