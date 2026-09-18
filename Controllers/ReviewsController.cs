using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.DTOs.Reviews;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Marketplace kitoblariga sharh va reyting.
/// Route: /api/salebooks/{saleBookId}/reviews
/// </summary>
[ApiController]
[Route("api/salebooks/{saleBookId:int}/reviews")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(AppDbContext context, ILogger<ReviewsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ======== GET — kitobning barcha sharhlari (ochiq) ========
    [HttpGet]
    public async Task<ActionResult<ReviewsListDto>> GetReviews(int saleBookId, CancellationToken ct = default)
    {
        var bookExists = await _context.SaleBooks.AnyAsync(b => b.Id == saleBookId, ct);
        if (!bookExists) return NotFound(new { message = "Kitob topilmadi" });

        var reviews = await _context.Reviews
            .Where(r => r.SaleBookId == saleBookId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                AuthorName = r.AuthorName,
                Content = r.Content,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId
            })
            .ToListAsync(ct);

        var avg = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;

        return Ok(new ReviewsListDto
        {
            SaleBookId = saleBookId,
            TotalReviews = reviews.Count,
            AverageRating = avg,
            Reviews = reviews
        });
    }

    // ======== POST — sharh qo'shish (login kerak) ========
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview(int saleBookId, [FromBody] CreateReviewDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized(new { message = "Foydalanuvchi aniqlanmadi" });

        var user = await _context.Users.FindAsync(new object?[] { userId.Value }, ct);
        if (user is null) return Unauthorized(new { message = "Foydalanuvchi topilmadi" });

        var bookExists = await _context.SaleBooks.AnyAsync(b => b.Id == saleBookId, ct);
        if (!bookExists) return NotFound(new { message = "Kitob topilmadi" });

        var already = await _context.Reviews
            .AnyAsync(r => r.SaleBookId == saleBookId && r.UserId == userId, ct);
        if (already)
            return Conflict(new { message = "Siz bu kitobga allaqachon sharh qoldirgansiz" });

        var review = new Review
        {
            SaleBookId = saleBookId,
            UserId = userId.Value,
            AuthorName = user.FullName,
            Content = dto.Content.Trim(),
            Rating = dto.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Yangi sharh: KitobId={BookId}, UserId={UserId}", saleBookId, userId);
        return Created($"/api/salebooks/{saleBookId}/reviews/{review.Id}", review.ToDto());
    }

    // ======== DELETE — muallif yoki admin ========
    [HttpDelete("{reviewId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int saleBookId, int reviewId, CancellationToken ct = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.SaleBookId == saleBookId, ct);
        if (review is null) return NotFound(new { message = "Sharh topilmadi" });

        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        if (!User.IsAdmin() && review.UserId != userId)
            return Forbid();

        review.DeletedByUserId = userId;
        _context.Reviews.Remove(review); // Soft delete
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Sharh o'chirildi: Id={ReviewId}, O'chirgan={UserId}", reviewId, userId);
        return NoContent();
    }
}
