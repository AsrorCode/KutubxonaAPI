using KutubxonaAPI.Common.Constants;
using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.DTOs.Orders;
using KutubxonaAPI.Models;
using KutubxonaAPI.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Buyurtmalar bilan ishlash uchun kontroller.
/// Buyurtma yaratish (transactional + concurrency safe),
/// tarixni ko'rish, status yangilash.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // POST /api/orders — Buyurtma yaratish
    // Transaction + Retry + Concurrency himoyasi
    // ============================================
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "Buyurtmada hech bo'lmaganda 1 ta kitob bo'lishi kerak" });

        var userId = User.GetUserId() ?? 0;
        if (userId == 0) return Unauthorized();

        // Bir necha marta urinish (concurrency conflict bo'lganda)
        for (int attempt = 1; attempt <= OrderConstants.MaxOrderRetries; attempt++)
        {
            try
            {
                var result = await CreateOrderTransactional(userId, dto);
                if (result is CreatedResult) return result;
                return result; // BadRequest, NotFound va h.k.
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    "Concurrency conflict, urinish {Attempt}/{Max}: {Message}",
                    attempt, OrderConstants.MaxOrderRetries, ex.Message);

                if (attempt == OrderConstants.MaxOrderRetries)
                {
                    return Conflict(new
                    {
                        message = "Kitob boshqa foydalanuvchi tomonidan olindi. Qayta uring."
                    });
                }

                // Kontekstni tozalash — keyingi urinishda toza state
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                {
                    entry.State = EntityState.Detached;
                }

                await Task.Delay(OrderConstants.RetryBackoffMs * attempt); // Progressive backoff
            }
        }

        return StatusCode(500, new { message = "Buyurtma yaratib bo'lmadi" });
    }

    // ============================================
    // Ichki metod — transaction ichida bajariladi
    // ============================================
    private async Task<IActionResult> CreateOrderTransactional(int userId, CreateOrderDto dto)
    {
        // Transaction boshlash — atomik operatsiya
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted);

        try
        {
            // Kitoblarni olish (RowVersion bilan)
            var bookIds = dto.Items.Select(i => i.SaleBookId).Distinct().ToList();
            var books = await _context.SaleBooks
                .Where(b => bookIds.Contains(b.Id) && b.IsActive)
                .ToListAsync();

            if (books.Count != bookIds.Count)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Ba'zi kitoblar topilmadi yoki faol emas" });
            }

            // Stock tekshirish
            foreach (var item in dto.Items)
            {
                var book = books.First(b => b.Id == item.SaleBookId);

                if (item.Quantity <= 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = "Miqdor 0 dan katta bo'lishi kerak" });
                }

                if (book.Stock < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = $"\"{book.Title}\" — yetarli zaxira yo'q. Mavjud: {book.Stock}, so'ralgan: {item.Quantity}"
                    });
                }
            }

            // Buyurtma yaratish
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                CustomerName = dto.CustomerName.Trim(),
                CustomerPhone = dto.CustomerPhone.Trim(),
                DeliveryAddress = dto.DeliveryAddress.Trim(),
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            decimal total = 0;
            foreach (var item in dto.Items)
            {
                var book = books.First(b => b.Id == item.SaleBookId);
                var orderItem = new OrderItem
                {
                    SaleBookId = book.Id,
                    Quantity = item.Quantity,
                    PriceAtOrder = book.Price
                };
                order.Items.Add(orderItem);
                total += book.Price * item.Quantity;

                // Zaxirani kamaytirish (RowVersion tekshiriladi)
                book.Stock -= item.Quantity;
                book.UpdatedAt = DateTime.UtcNow;
            }
            order.TotalAmount = total;

            _context.Orders.Add(order);

            // SaveChanges — agar RowVersion o'zgargan bo'lsa exception uloqtiradi
            await _context.SaveChangesAsync();

            // Muvaffaqiyat — transaction'ni commit qilish
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Buyurtma yaratildi: OrderId={OrderId}, UserId={UserId}, Total={Total}, Items={Count}",
                order.Id, userId, total, order.Items.Count);

            return Created($"/api/orders/{order.Id}", order.ToCreatedDto());
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw; // Yuqoriga retry uchun tashlab yuborish
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Buyurtma yaratishda xato: UserId={UserId}", userId);
            return StatusCode(500, new { message = "Buyurtma yaratishda xato yuz berdi" });
        }
    }

    // ============================================
    // GET /api/orders/my — Foydalanuvchi buyurtmalari
    // ============================================
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.GetUserId() ?? 0;
        if (userId == 0) return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => o.ToSummaryDto()));
    }

    // ============================================
    // GET /api/orders (Admin)
    // ============================================
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var orders = await query
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => o.ToDetailDto()));
    }

    // ============================================
    // GET /api/orders/{id}
    // ============================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        var userId = User.GetUserId() ?? 0;
        var isAdmin = User.IsAdmin();
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return Ok(order.ToDetailDto());
    }

    // ============================================
    // PATCH /api/orders/{id}/status (Admin)
    // Buyurtma bekor qilinsa — stock qaytariladi
    // ============================================
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
    {
        // Enum'ga parse qilish — kompilyatsiya emas, runtime tekshirish
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var newStatus))
        {
            return BadRequest(new
            {
                message = "Status noto'g'ri",
                valid = Enum.GetNames<OrderStatus>()
            });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var oldStatus = order.Status;

            // Bekor qilinsa — stock qaytariladi
            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var book = await _context.SaleBooks.FindAsync(item.SaleBookId);
                    if (book != null)
                    {
                        book.Stock += item.Quantity;
                        book.UpdatedAt = DateTime.UtcNow;
                    }
                }

                _logger.LogInformation("Buyurtma bekor qilindi, stock qaytarildi: OrderId={OrderId}", id);
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { order.Id, order.Status });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Status yangilashda xato: OrderId={OrderId}", id);
            return StatusCode(500, new { message = "Status yangilanmadi" });
        }
    }
}

// ============================================
// DTOs
// ============================================
public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int SaleBookId { get; set; }
    public int Quantity { get; set; } = 1;
}
