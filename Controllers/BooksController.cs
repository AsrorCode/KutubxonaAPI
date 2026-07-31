using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using KutubxonaAPI.Data;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Kitoblar bilan ishlash uchun API Controller.
///
/// Route: /api/books
///
/// CRUD amallar:
/// - GET    /api/books         → Barcha kitoblarni olish
/// - GET    /api/books/{id}    → Bitta kitobni olish
/// - GET    /api/books/search  → Qidiruv
/// - POST   /api/books         → Yangi kitob qo'shish
/// - PUT    /api/books/{id}    → Kitobni yangilash
/// - DELETE /api/books/{id}    → Kitobni o'chirish
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BooksController> _logger;

    /// <summary>
    /// Constructor - DbContext va Logger DI orqali keladi.
    /// </summary>
    public BooksController(AppDbContext context, ILogger<BooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==========================================
    // READ - Barcha kitoblarni olish
    // ==========================================

    /// <summary>
    /// Barcha kitoblarni qaytaradi.
    /// </summary>
    /// <returns>Kitoblar ro'yxati</returns>
    /// <response code="200">Muvaffaqiyatli</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        _logger.LogInformation("Barcha kitoblar so'ralmoqda");

        var books = await _context.Books
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return Ok(books);
    }

    // ==========================================
    // READ - Bitta kitobni ID bo'yicha olish
    // ==========================================

    /// <summary>
    /// ID bo'yicha bitta kitobni qaytaradi.
    /// </summary>
    /// <param name="id">Kitob ID raqami</param>
    /// <returns>Kitob obyekti</returns>
    /// <response code="200">Kitob topildi</response>
    /// <response code="404">Kitob topilmadi</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            _logger.LogWarning("Kitob topilmadi. ID: {Id}", id);
            return NotFound(new { message = $"ID = {id} bo'lgan kitob topilmadi" });
        }

        return Ok(book);
    }

    // ==========================================
    // READ - Qidiruv
    // ==========================================

    /// <summary>
    /// Kitob nomi yoki muallif bo'yicha qidiradi.
    /// </summary>
    /// <param name="query">Qidiruv so'zi</param>
    /// <returns>Mos keladigan kitoblar</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetBooks();
        }

        var lowerQuery = query.ToLower();

        var books = await _context.Books
            .Where(b => b.Title.ToLower().Contains(lowerQuery)
                     || b.Author.ToLower().Contains(lowerQuery)
                     || b.Category.ToLower().Contains(lowerQuery))
            .ToListAsync();

        return Ok(books);
    }

    // ==========================================
    // CREATE - Yangi kitob qo'shish
    // ==========================================

    /// <summary>
    /// Yangi kitob qo'shadi.
    /// </summary>
    /// <param name="book">Yangi kitob ma'lumotlari</param>
    /// <returns>Yaratilgan kitob (ID bilan)</returns>
    /// <response code="201">Kitob muvaffaqiyatli yaratildi</response>
    /// <response code="400">Noto'g'ri ma'lumot</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
    {
        // Model validatsiyasi (DataAnnotations orqali)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // ID'ni avtomatik berishi uchun 0 qilamiz
        book.Id = 0;
        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = null;

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi kitob qo'shildi: {Title}", book.Title);

        // 201 Created javobini qaytaramiz, headerda yangi resurs URL'i bilan
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // ==========================================
    // UPDATE - Kitobni yangilash
    // ==========================================

    /// <summary>
    /// Mavjud kitobni yangilaydi.
    /// </summary>
    /// <param name="id">Yangilanadigan kitob ID si</param>
    /// <param name="updatedBook">Yangi ma'lumotlar</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Book>> UpdateBook(int id, [FromBody] Book updatedBook)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingBook = await _context.Books.FindAsync(id);

        if (existingBook == null)
        {
            return NotFound(new { message = $"ID = {id} bo'lgan kitob topilmadi" });
        }

        // Maydonlarni yangilash
        existingBook.Title = updatedBook.Title;
        existingBook.Author = updatedBook.Author;
        existingBook.Year = updatedBook.Year;
        existingBook.Category = updatedBook.Category;
        existingBook.IsAvailable = updatedBook.IsAvailable;
        existingBook.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Kitob yangilandi. ID: {Id}", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Bir vaqtda bir nechta o'zgartirish bo'lsa
            return Conflict(new { message = "Kitob boshqa joyda o'zgartirilgan" });
        }

        return Ok(existingBook);
    }

    // ==========================================
    // DELETE - Kitobni o'chirish
    // ==========================================

    /// <summary>
    /// Kitobni o'chiradi.
    /// </summary>
    /// <param name="id">O'chiriladigan kitob ID si</param>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"ID = {id} bo'lgan kitob topilmadi" });
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kitob o'chirildi. ID: {Id}, Nomi: {Title}", id, book.Title);

        // 204 No Content - muvaffaqiyatli o'chirildi, qaytariladigan ma'lumot yo'q
        return NoContent();
    }

    // ==========================================
    // PARTIAL UPDATE - Faqat holat (status) ni yangilash
    // ==========================================

    /// <summary>
    /// Faqat kitob holatini (mavjud/olingan) o'zgartiradi.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> UpdateStatus(int id, [FromQuery] bool isAvailable)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound(new { message = $"ID = {id} bo'lgan kitob topilmadi" });
        }

        book.IsAvailable = isAvailable;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(book);
    }
}
