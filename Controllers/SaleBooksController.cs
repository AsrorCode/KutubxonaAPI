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

        return Ok(books.Select(b => b.ToDto()));
    }

    // ======== GET /api/salebooks/{id} ========
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleBookResponseDto>> GetOne(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });
        return Ok(book.ToDto());
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
            CreatedAt = DateTime.UtcNow
        };

        _context.SaleBooks.Add(book);
        await _context.SaveChangesAsync();

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
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(book.ToDto());
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
