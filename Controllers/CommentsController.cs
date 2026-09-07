using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    // GET - Kitobning barcha izohlari (public)
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
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.AuthorName,
                c.Content,
                c.Rating,
                c.CreatedAt,
                c.UserId
            })
            .ToListAsync();

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
    // POST - Yangi izoh qo'shish (AUTH majburiy)
    // ==========================================

    /// <summary>
    /// Kitobga yangi izoh qo'shadi. FAQAT autentifikatsiya qilingan userlar.
    /// AuthorName avtomatik JWT tokendan olinadi.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment(int bookId, [FromBody] CreateCommentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // JWT tokendan user ID va ismni olish
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Foydalanuvchi identifikatsiyasi topilmadi" });
        }

        // Foydalanuvchi hali mavjudmi tekshirish
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { message = "Foydalanuvchi topilmadi" });
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

        // Bir foydalanuvchi bir kitobga faqat 1 marta izoh yozishi mumkinmi tekshirish
        var alreadyCommented = await _context.Comments
            .AnyAsync(c => c.BookId == bookId && c.UserId == userId);

        if (alreadyCommented)
        {
            return Conflict(new { message = "Siz bu kitobga allaqachon izoh qoldirgansiz" });
        }

        var newComment = new Comment
        {
            BookId = bookId,
            UserId = userId,
            AuthorName = user.FullName,  // Avtomatik ismdan olindi
            Content = dto.Content,
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(newComment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Yangi izoh qo'shildi. Kitob: {BookId}, User: {UserId} ({Name})",
            bookId, userId, user.FullName);

        return Created($"/api/books/{bookId}/comments/{newComment.Id}", new
        {
            newComment.Id,
            newComment.AuthorName,
            newComment.Content,
            newComment.Rating,
            newComment.CreatedAt,
            newComment.UserId
        });
    }

    // ==========================================
    // DELETE - Izohni o'chirish
    // Muallif o'z izohini yoki Admin — istagan izohni o'chira oladi
    // ==========================================

    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int bookId, int commentId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.BookId == bookId);

        if (comment == null)
        {
            return NotFound(new { message = "Izoh topilmadi" });
        }

        // Tekshirish: Admin YOKI izoh muallifi
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        if (!isAdmin && comment.UserId != userId)
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Izoh o'chirildi. ID: {CommentId}, O'chirgan: {UserId}, Admin: {IsAdmin}",
            commentId, userId, isAdmin);

        return NoContent();
    }
}

// ==========================================
// DTO - Izoh qo'shish uchun
// AuthorName endi kerak emas — JWT'dan olinadi
// ==========================================

public class CreateCommentDto
{
    /// <summary>Izoh matni</summary>
    [Required(ErrorMessage = "Izoh matni kiritilishi shart")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Izoh 3-1000 belgi bo'lishi kerak")]
    public string Content { get; set; } = string.Empty;

    /// <summary>Reyting (1-5)</summary>
    [Range(1, 5, ErrorMessage = "Reyting 1 va 5 oralig'ida bo'lishi kerak")]
    public int Rating { get; set; }
}
