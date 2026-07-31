using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KutubxonaAPI.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize]  // Hamma endpoint authsiz emas
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetUserId()
    {
        var idStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    // ======== POST /api/orders (User) ========
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "Buyurtmada hech bo'lmaganda 1 ta kitob bo'lishi kerak" });

        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        // Buyurtmadagi kitoblarni database'dan olamiz
        var bookIds = dto.Items.Select(i => i.SaleBookId).ToList();
        var books = await _context.SaleBooks
            .Where(b => bookIds.Contains(b.Id) && b.IsActive)
            .ToListAsync();

        if (books.Count != bookIds.Count)
            return BadRequest(new { message = "Ba'zi kitoblar topilmadi yoki faol emas" });

        // Yetarli zaxira borligini tekshirish
        foreach (var item in dto.Items)
        {
            var book = books.First(b => b.Id == item.SaleBookId);
            if (book.Stock < item.Quantity)
                return BadRequest(new { message = $"\"{book.Title}\" — yetarli zaxira yo'q ({book.Stock} dona)" });
        }

        // Buyurtma yaratish
        var order = new Order
        {
            UserId = userId,
            Status = "Pending",
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

            // Zaxirani kamaytirish
            book.Stock -= item.Quantity;
        }
        order.TotalAmount = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi buyurtma: ID={OrderId}, UserId={UserId}, Summa={Total}",
            order.Id, userId, total);

        return Created($"/api/orders/{order.Id}", new
        {
            order.Id,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            ItemsCount = order.Items.Count
        });
    }

    // ======== GET /api/orders/my (User) ========
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                ItemsCount = o.Items.Count,
                Items = o.Items.Select(i => new
                {
                    i.SaleBookId,
                    BookTitle = i.SaleBook!.Title,
                    BookAuthor = i.SaleBook.Author,
                    i.Quantity,
                    i.PriceAtOrder,
                    Subtotal = i.Quantity * i.PriceAtOrder
                })
            })
            .ToListAsync();

        return Ok(orders);
    }

    // ======== GET /api/orders (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.Status,
                o.CustomerName,
                o.CustomerPhone,
                o.DeliveryAddress,
                o.CreatedAt,
                Customer = new { o.User!.Email, o.User.FirstName, o.User.LastName },
                ItemsCount = o.Items.Count
            })
            .ToListAsync();

        return Ok(orders);
    }

    // ======== GET /api/orders/{id} ========
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.SaleBook)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // User faqat o'z buyurtmasini ko'ra oladi
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        return Ok(new
        {
            order.Id,
            order.TotalAmount,
            order.Status,
            order.CustomerName,
            order.CustomerPhone,
            order.DeliveryAddress,
            order.Notes,
            order.CreatedAt,
            Items = order.Items.Select(i => new
            {
                i.SaleBookId,
                BookTitle = i.SaleBook!.Title,
                BookAuthor = i.SaleBook.Author,
                i.Quantity,
                i.PriceAtOrder,
                Subtotal = i.Quantity * i.PriceAtOrder
            })
        });
    }

    // ======== PATCH /api/orders/{id}/status (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
    {
        var validStatuses = new[] { "Pending", "Paid", "Shipped", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(status))
            return BadRequest(new { message = "Status noto'g'ri", valid = validStatuses });

        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { order.Id, order.Status });
    }
}

// DTOs
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