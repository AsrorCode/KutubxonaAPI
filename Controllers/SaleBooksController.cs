using KutubxonaAPI.Controllers.Data;
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
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _context.SaleBooks.AsQueryable();
        if (!includeInactive) query = query.Where(b => b.IsActive);

        var books = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(books);
    }

    // ======== GET /api/salebooks/{id} ========
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound(new { message = "Kitob topilmadi" });
        return Ok(book);
    }

    // ======== GET /api/salebooks/search ========
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "", [FromQuery] string? category = null)
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
        return Ok(books);
    }

    // ======== POST /api/salebooks (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleBookDto dto)
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.SaleBooks.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi sotuv kitobi qo'shildi: {Title}", book.Title);
        return CreatedAtAction(nameof(GetOne), new { id = book.Id }, book);
    }

    // ======== PUT /api/salebooks/{id} (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleBookDto dto)
    {
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
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(book);
    }

    // ======== DELETE /api/salebooks/{id} (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound();

        _context.SaleBooks.Remove(book);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ======== PATCH /api/salebooks/{id}/status (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ToggleActive(int id, [FromQuery] bool isActive)
    {
        var book = await _context.SaleBooks.FindAsync(id);
        if (book == null) return NotFound();

        book.IsActive = isActive;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(book);
    }
}

// DTO
public class SaleBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public int? Year { get; set; }
}