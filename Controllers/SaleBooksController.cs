using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.DTOs.SaleBooks;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

[ApiController]
[Route("api/salebooks")]
[Produces("application/json")]
public class SaleBooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SaleBooksController> _logger;

    public SaleBooksController(AppDbContext context, ILogger<SaleBooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ======== GET /api/salebooks ========
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleBookResponseDto>>> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var query = _context.SaleBooks.AsQueryable();
        if (!includeInactive) query = query.Where(b => b.IsActive);

        var books = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        // Har kitob uchun sotilgan soni (ijtimoiy dalil)
        var bookIds = books.Select(b => b.Id).ToList();
        var soldMap = await _context.OrderItems
            .Where(oi => bookIds.Contains(oi.SaleBookId))
            .GroupBy(oi => oi.SaleBookId)
            .Select(g => new { Id = g.Key, Sold = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.Id, x => x.Sold);

        var dtos = books.Select(b =>
        {
            var dto = b.ToDto();
            dto.SoldCount = soldMap.GetValueOrDefault(b.Id, 0);
            return dto;
        });

        return Ok(dtos);
    }

    // ======== GET /api/salebooks/bestsellers ========
    /// <summary>
    /// Eng ko'p sotilgan kitoblar — buyurtmalardagi jami miqdor bo'yicha.
    /// Sotuv bo'lmasa, eng yangi faol kitoblar qaytariladi.
    /// </summary>
    [HttpGet("bestsellers")]
    public async Task<ActionResult<IEnumerable<object>>> GetBestsellers(
        [FromQuery] int count = 6,
        CancellationToken ct = default)
    {
        if (count < 1) count = 6;
        if (count > 20) count = 20;

        // OrderItem'lar bo'yicha sotilgan miqdorni hisoblash
        var soldCounts = await _context.OrderItems
            .GroupBy(oi => oi.SaleBookId)
            .Select(g => new { SaleBookId = g.Key, Sold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Sold)
            .Take(count)
            .ToListAsync(ct);

        var soldMap = soldCounts.ToDictionary(x => x.SaleBookId, x => x.Sold);
        var bestsellerIds = soldCounts.Select(x => x.SaleBookId).ToList();

        var books = await _context.SaleBooks
            .Where(b => b.IsActive && bestsellerIds.Contains(b.Id))
            .ToListAsync(ct);

        // Sotuv tartibida
        var ordered = books
            .OrderByDescending(b => soldMap.GetValueOrDefault(b.Id, 0))
            .Select(b => new
            {
                book = b.ToDto(),
                soldCount = soldMap.GetValueOrDefault(b.Id, 0)
            })
            .ToList();

        // Agar sotuv yetarli bo'lmasa — eng yangilar bilan to'ldirish
        if (ordered.Count < count)
        {
            var existingIds = ordered.Select(o => o.book.Id).ToHashSet();
            var fillers = await _context.SaleBooks
                .Where(b => b.IsActive && !existingIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedAt)
                .Take(count - ordered.Count)
                .ToListAsync(ct);
            ordered.AddRange(fillers.Select(b => new { book = b.ToDto(), soldCount = 0 }));
        }

        return Ok(ordered);
    }

    // ======== GET /api/salebooks/{id}/detail ========
    /// <summary>
    /// Mahsulot to'liq sahifasi uchun: kitob + sotilgan soni + kategoriyadagi o'rni.
    /// </summary>
    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<object>> GetDetail(int id, CancellationToken ct = default)
    {
        var book = await _context.SaleBooks.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });

        // Ko'rishlar sonini atomik oshirish (RowVersion muammosisiz)
        await _context.SaleBooks
            .Where(b => b.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ViewCount, b => b.ViewCount + 1), ct);
        book.ViewCount++; // javobда yangi qiymat

        // Sotilgan soni
        var soldCount = await _context.OrderItems
            .Where(oi => oi.SaleBookId == id)
            .SumAsync(oi => (int?)oi.Quantity, ct) ?? 0;

        // So'nggi 7 kunda sotilgan
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var soldThisWeek = await _context.OrderItems
            .Where(oi => oi.SaleBookId == id && oi.Order!.CreatedAt >= weekAgo)
            .SumAsync(oi => (int?)oi.Quantity, ct) ?? 0;

        // Kategoriyadagi sotuv reytingi (#N-o'rin)
        var categorySales = await _context.OrderItems
            .Where(oi => oi.SaleBook!.Category == book.Category)
            .GroupBy(oi => oi.SaleBookId)
            .Select(g => new { Id = g.Key, Sold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Sold)
            .ToListAsync(ct);

        var rank = categorySales.FindIndex(x => x.Id == id) + 1;

        return Ok(new
        {
            book = book.ToDto(),
            soldCount,
            soldThisWeek,
            categoryRank = rank > 0 ? rank : (int?)null,
            category = book.Category
        });
    }

    // ======== GET /api/salebooks/for-you — shaxsiy tavsiya ========
    /// <summary>
    /// Foydalanuvchining buyurtma va sevimli kategoriyalariga qarab tavsiya.
    /// Yangi foydalanuvchi — eng yangi/ommabop kitoblar.
    /// </summary>
    [Authorize]
    [HttpGet("for-you")]
    public async Task<ActionResult<IEnumerable<SaleBookResponseDto>>> ForYou(
        [FromQuery] int count = 8, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();
        if (count < 1) count = 8;
        if (count > 20) count = 20;

        var boughtBookIds = await _context.OrderItems
            .Where(oi => oi.Order!.UserId == userId)
            .Select(oi => oi.SaleBookId).Distinct().ToListAsync(ct);

        var wishBookIds = await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Select(w => w.SaleBookId).ToListAsync(ct);

        var seedIds = boughtBookIds.Concat(wishBookIds).Distinct().ToList();

        var favCategories = await _context.SaleBooks
            .Where(b => seedIds.Contains(b.Id))
            .Select(b => b.Category).Distinct().ToListAsync(ct);

        var recs = new List<SaleBook>();
        if (favCategories.Count > 0)
        {
            recs = await _context.SaleBooks
                .Where(b => b.IsActive && favCategories.Contains(b.Category) && !boughtBookIds.Contains(b.Id))
                .OrderByDescending(b => b.ViewCount).ThenByDescending(b => b.CreatedAt)
                .Take(count).ToListAsync(ct);
        }

        // To'ldirish — yangilar
        if (recs.Count < count)
        {
            var have = recs.Select(r => r.Id).Concat(boughtBookIds).ToHashSet();
            var fillers = await _context.SaleBooks
                .Where(b => b.IsActive && !have.Contains(b.Id))
                .OrderByDescending(b => b.CreatedAt)
                .Take(count - recs.Count).ToListAsync(ct);
            recs.AddRange(fillers);
        }

        return Ok(recs.Select(b => b.ToDto()));
    }

    // ======== GET /api/salebooks/{id}/similar ========
    /// <summary>
    /// O'xshash kitoblar — shu kategoriyadagilar (o'zini chiqarib).
    /// Yetarli bo'lmasa, boshqa kategoriyalardan to'ldiriladi.
    /// </summary>
    [HttpGet("{id:int}/similar")]
    public async Task<ActionResult<IEnumerable<SaleBookResponseDto>>> GetSimilar(
        int id, [FromQuery] int count = 6, CancellationToken ct = default)
    {
        var book = await _context.SaleBooks.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });

        if (count < 1) count = 6;
        if (count > 20) count = 20;

        var similar = await _context.SaleBooks
            .Where(b => b.IsActive && b.Id != id && b.Category == book.Category)
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

        // Yetarli bo'lmasa — boshqa kategoriyalardan to'ldirish
        if (similar.Count < count)
        {
            var excludeIds = similar.Select(s => s.Id).ToHashSet();
            excludeIds.Add(id);
            var fillers = await _context.SaleBooks
                .Where(b => b.IsActive && !excludeIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedAt)
                .Take(count - similar.Count)
                .ToListAsync(ct);
            similar.AddRange(fillers);
        }

        return Ok(similar.Select(b => b.ToDto()));
    }

    // ======== GET /api/salebooks/{id} ========
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleBookResponseDto>> GetOne(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });
        return Ok(book.ToDto());
    }

    // ======== GET /api/salebooks/suggest — autocomplete ========
    /// <summary>Qidiruv uchun jonli takliflar (top 6 mos kitob).</summary>
    [HttpGet("suggest")]
    public async Task<ActionResult<IEnumerable<object>>> Suggest(
        [FromQuery] string q = "", CancellationToken ct = default)
    {
        var term = (q ?? "").Trim();
        if (term.Length < 2) return Ok(Array.Empty<object>());

        var lower = term.ToLower();
        var books = await _context.SaleBooks
            .Where(b => b.IsActive && (b.Title.ToLower().Contains(lower) || b.Author.ToLower().Contains(lower)))
            .OrderByDescending(b => b.CreatedAt)
            .Take(6)
            .Select(b => new { id = b.Id, title = b.Title, author = b.Author })
            .ToListAsync(ct);

        return Ok(books);
    }

    // ======== GET /api/salebooks/search ========
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SaleBookResponseDto>>> Search(
        [FromQuery] string q = "",
        [FromQuery] string? category = null)
    {
        var query = _context.SaleBooks.Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var lower = q.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(lower) ||
                b.Author.ToLower().Contains(lower) ||
                b.Description.ToLower().Contains(lower));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        var books = await query.ToListAsync();
        return Ok(books.Select(b => b.ToDto()));
    }

    // ======== POST /api/salebooks (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SaleBookResponseDto>> Create([FromBody] SaleBookCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var book = new SaleBook
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            Description = dto.Description ?? "",
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl ?? "",
            Category = dto.Category ?? "Boshqa",
            Year = dto.Year,
            IsActive = dto.IsActive,
            Discount = dto.Discount,
            DiscountEndsAt = dto.DiscountEndsAt,
            Publisher = dto.Publisher ?? "",
            CoverType = dto.CoverType ?? "",
            PageCount = dto.PageCount,
            Isbn = dto.Isbn ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _context.SaleBooks.Add(book);
        await _context.SaveChangesAsync();

        await AddBookNotificationAsync(book, isNew: true);

        _logger.LogInformation("Yangi sotuv kitobi qo'shildi: {Title}", book.Title);
        return CreatedAtAction(nameof(GetOne), new { id = book.Id }, book.ToDto());
    }

    // ======== PUT /api/salebooks/{id} (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SaleBookResponseDto>> Update(int id, [FromBody] SaleBookCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });

        var oldDiscount = book.Discount;

        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.Description = dto.Description ?? "";
        book.Price = dto.Price;
        book.Stock = dto.Stock;
        book.ImageUrl = dto.ImageUrl ?? "";
        book.Category = dto.Category ?? book.Category;
        book.Year = dto.Year;
        book.IsActive = dto.IsActive;
        book.Discount = dto.Discount;
        book.DiscountEndsAt = dto.DiscountEndsAt;
        book.Publisher = dto.Publisher ?? "";
        book.CoverType = dto.CoverType ?? "";
        book.PageCount = dto.PageCount;
        book.Isbn = dto.Isbn ?? "";
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Yangi aksiya qo'yilganda (0 dan >0 ga) bildirishnoma
        if (oldDiscount == 0 && book.Discount > 0)
            await AddBookNotificationAsync(book, isNew: false);

        return Ok(book.ToDto());
    }

    // ======== Yordamchi: aksiya/yangi kitob bildirishnomasi ========
    private async Task AddBookNotificationAsync(SaleBook book, bool isNew)
    {
        if (!book.IsActive) return;

        Notification n;
        if (book.Discount > 0)
        {
            var final = Math.Round(book.Price - (book.Price * book.Discount / 100m), 0);
            n = new Notification
            {
                Title = "🔥 Yangi aksiya!",
                Message = $"{book.Title} — endi {final:#,0} so'm ({book.Discount}% chegirma)",
                Type = "discount",
                SaleBookId = book.Id,
                CreatedAt = DateTime.UtcNow
            };
        }
        else if (isNew)
        {
            n = new Notification
            {
                Title = "🆕 Yangi kitob",
                Message = $"{book.Title} — {book.Author}",
                Type = "new",
                SaleBookId = book.Id,
                CreatedAt = DateTime.UtcNow
            };
        }
        else return;

        _context.Notifications.Add(n);
        await _context.SaveChangesAsync();
    }

    // ======== DELETE /api/salebooks/{id} — Soft Delete (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound();

        book.DeletedByUserId = User.GetUserId();
        _context.SaleBooks.Remove(book); // Auto soft delete
        await _context.SaveChangesAsync();

        _logger.LogInformation("Marketplace kitob o'chirildi (soft): Id={Id}", id);
        return NoContent();
    }

    // ======== GET /api/salebooks/trash (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpGet("trash")]
    public async Task<ActionResult<IEnumerable<SaleBookResponseDto>>> GetDeleted()
    {
        var books = await _context.SaleBooks
            .IgnoreQueryFilters()
            .Where(b => b.IsDeleted)
            .OrderByDescending(b => b.DeletedAt)
            .ToListAsync();

        return Ok(books.Select(b => b.ToDto()));
    }

    // ======== POST /api/salebooks/{id}/restore (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/restore")]
    public async Task<ActionResult<SaleBookResponseDto>> Restore(int id)
    {
        var book = await _context.SaleBooks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return NotFound();
        if (!book.IsDeleted) return BadRequest(new { message = "Kitob o'chirilmagan" });

        book.IsDeleted = false;
        book.DeletedAt = null;
        book.DeletedByUserId = null;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Marketplace kitob tiklandi: Id={Id}", id);
        return Ok(book.ToDto());
    }

    // ======== PATCH /api/salebooks/{id}/toggle (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/toggle")]
    public async Task<ActionResult<SaleBookResponseDto>> ToggleActive(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound();

        book.IsActive = !book.IsActive;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(book.ToDto());
    }
}
