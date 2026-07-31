using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BooksController> _logger;

    public BooksController(AppDbContext context, ILogger<BooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // GET: /api/books?page=1&pageSize=20&category=Klassika&search=Qodiriy
    // ============================================
    [HttpGet]
    public async Task<ActionResult<PagedResult<Book>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null)
    {
        // Pagination cheklovlari
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Books.AsQueryable();

        // Filter — kategoriya
        if (!string.IsNullOrWhiteSpace(category) && category != "all")
            query = query.Where(b => b.Category == category);

        // Filter — qidiruv
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(searchLower) ||
                b.Author.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<Book>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // ============================================
    // GET: /api/books/{id}
    // ============================================
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        return Ok(book);
    }

    // ============================================
    // GET: /api/books/categories — barcha kategoriyalar
    // ============================================
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    // ============================================
    // POST: /api/books — Yangi kitob (ADMIN)
    // ============================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi kitob qo'shildi: {Title}", book.Title);

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // ============================================
    // PUT: /api/books/{id} — To'liq yangilash (ADMIN)
    // ============================================
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
    {
        if (id != updatedBook.Id)
            return BadRequest(new { message = "ID mos kelmayapti" });

        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.Year = updatedBook.Year;
        book.Category = updatedBook.Category;
        book.IsAvailable = updatedBook.IsAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Kitob yangilandi: {Id}", id);

        return NoContent();
    }

    // ============================================
    // PATCH: /api/books/{id}/status — Faqat status (ADMIN)
    // ============================================
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isAvailable)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        book.IsAvailable = isAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Status yangilandi", isAvailable });
    }

    // ============================================
    // DELETE: /api/books/{id} — O'chirish (ADMIN)
    // ============================================
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kitob o'chirildi: {Id}", id);

        return NoContent();
    }
}