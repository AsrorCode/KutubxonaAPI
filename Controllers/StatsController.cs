using KutubxonaAPI.Data;
using KutubxonaAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Admin statistika paneli uchun yig'ma ma'lumotlar.
/// </summary>
[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var since14 = now.Date.AddDays(-13); // bugun bilan 14 kun

        var nonCancelled = _context.Orders.Where(o => o.Status != OrderStatus.Cancelled);

        var totalRevenue = await nonCancelled.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;
        var monthRevenue = await nonCancelled
            .Where(o => o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

        var totalOrders = await _context.Orders.CountAsync(ct);

        var statusCounts = await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var byStatus = statusCounts.ToDictionary(x => x.Status.ToString(), x => x.Count);

        var activeBooks = await _context.SaleBooks.CountAsync(b => b.IsActive, ct);
        var lowStock = await _context.SaleBooks.CountAsync(b => b.IsActive && b.Stock > 0 && b.Stock < 5, ct);
        var outOfStock = await _context.SaleBooks.CountAsync(b => b.IsActive && b.Stock == 0, ct);

        var totalUsers = await _context.Users.CountAsync(ct);
        var totalReviews = await _context.Reviews.CountAsync(ct);
        var avgRating = totalReviews > 0
            ? Math.Round(await _context.Reviews.AverageAsync(r => (double)r.Rating, ct), 1)
            : 0;

        // Eng ko'p sotilgan top 5
        var topRaw = await _context.OrderItems
            .GroupBy(oi => oi.SaleBookId)
            .Select(g => new
            {
                SaleBookId = g.Key,
                Sold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.PriceAtOrder)
            })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync(ct);

        var topIds = topRaw.Select(x => x.SaleBookId).ToList();
        var titles = await _context.SaleBooks
            .Where(b => topIds.Contains(b.Id))
            .Select(b => new { b.Id, b.Title })
            .ToListAsync(ct);

        var topBooks = topRaw.Select(x => new
        {
            title = titles.FirstOrDefault(t => t.Id == x.SaleBookId)?.Title ?? "—",
            sold = x.Sold,
            revenue = x.Revenue
        });

        // Kam qolgan kitoblar (ogohlantirish uchun)
        var lowStockBooks = await _context.SaleBooks
            .Where(b => b.IsActive && b.Stock < 5)
            .OrderBy(b => b.Stock)
            .Take(10)
            .Select(b => new { b.Title, b.Stock })
            .ToListAsync(ct);

        // So'nggi 14 kun daromadi
        var recentOrders = await nonCancelled
            .Where(o => o.CreatedAt >= since14)
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync(ct);

        var revenueByDay = Enumerable.Range(0, 14).Select(i =>
        {
            var day = since14.Date.AddDays(i);
            var amount = recentOrders.Where(o => o.CreatedAt.Date == day).Sum(o => o.TotalAmount);
            return new { date = day.ToString("MM-dd"), amount };
        });

        return Ok(new
        {
            revenue = new { total = totalRevenue, thisMonth = monthRevenue },
            orders = new { total = totalOrders, byStatus },
            books = new { active = activeBooks, lowStock, outOfStock },
            users = new { total = totalUsers },
            reviews = new { total = totalReviews, avgRating },
            topBooks,
            lowStockBooks,
            revenueByDay
        });
    }
}
