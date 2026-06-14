using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Kitob izohlari (sharhlari) bilan ishlash uchun Controller.
/// Route: /api/books/{bookId}/comments
/// </summary>
[ApiController]
[Route("api/books/{bookId:int}/comments")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(AppDbContext context, ILogger<CommentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==========================================
    // GET - Kitobning barcha izohlari
    // ==========================================

    /// <summary>
    /// Kitobning barcha izohlarini qaytaradi (eng yangisi birinchi).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetComments(int bookId)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
        {
            return NotFound(new { message = $"ID = {bookId} bo'lgan kitob topilmadi" });
        }

        var comments = await _context.Comments
            .Where(c => c.BookId == bookId)
            .OrderByDescending(c => c.CreatedAt)  // eng yangisi birinchi
            .Select(c => new
            {
                c.Id,
                c.AuthorName,
                c.Content,
                c.Rating,
                c.CreatedAt
            })
            .ToListAsync();

        // O'rtacha reytingni hisoblash
        double averageRating = comments.Count > 0
            ? comments.Average(c => c.Rating)
            : 0;

        return Ok(new
        {
            bookId,
            totalComments = comments.Count,
            averageRating = Math.Round(averageRating, 1),
            comments
        });
    }

    // ==========================================
    // POST - Yangi izoh qo'shish
    // ==========================================

    /// <summary>
    /// Kitobga yangi izoh qo'shadi.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateComment(int bookId, [FromBody] CreateCommentDto dto)
    {
        // Model validatsiyasi
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Kitob mavjudligini tekshirish
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
        {
            return NotFound(new { message = $"ID = {bookId} bo'lgan kitob topilmadi" });
        }

        // Reyting tekshiruvi
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            return BadRequest(new { message = "Reyting 1 va 5 oralig'ida bo'lishi kerak" });
        }

        var newComment = new Comment
        {
            BookId = bookId,
            AuthorName = dto.AuthorName,
            Content = dto.Content,
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi izoh qo'shildi. Kitob: {BookId}, Muallif: {Author}",
            bookId, dto.AuthorName);

        return Created($"/api/books/{bookId}/comments/{newComment.Id}", new
        {
            newComment.Id,
            newComment.AuthorName,
            newComment.Content,
            newComment.Rating,
            newComment.CreatedAt
        });
    }

    // ==========================================
    // DELETE - Izohni o'chirish
    // ==========================================

    /// <summary>
    /// Izohni o'chiradi.
    /// </summary>

    [Authorize(Roles = "Admin")]
    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int bookId, int commentId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.BookId == bookId);

        if (comment == null)
        {
            return NotFound(new { message = "Izoh topilmadi" });
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Izoh o'chirildi. ID: {CommentId}", commentId);

        return NoContent();
    }
}

// ==========================================
// DTO - Izoh qo'shish uchun
// ==========================================

public class CreateCommentDto
{
    /// <summary>Izoh qoldirgan odam ismi</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Izoh matni</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Reyting (1-5)</summary>
    public int Rating { get; set; }
}