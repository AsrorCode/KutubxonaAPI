using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Foydalanuvchining sevimli kitoblari (wishlist) — bazada, qurilmalar aro.
/// </summary>
[ApiController]
[Route("api/wishlist")]
[Authorize]
[Produces("application/json")]
public class WishlistController : ControllerBase
{
    private readonly AppDbContext _context;

    public WishlistController(AppDbContext context)
    {
        _context = context;
    }

    // ======== GET — mening sevimlilarim (kitoblar) ========
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var books = await _context.WishlistItems
            .Where(w => w.UserId == userId && w.SaleBook != null && w.SaleBook.IsActive)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.SaleBook!)
            .ToListAsync(ct);

        return Ok(books.Select(b => b.ToDto()));
    }

    // ======== POST — qo'shish ========
    [HttpPost("{bookId:int}")]
    public async Task<IActionResult> Add(int bookId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var bookExists = await _context.SaleBooks.AnyAsync(b => b.Id == bookId, ct);
        if (!bookExists) return NotFound(new { message = "Kitob topilmadi" });

        var already = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.SaleBookId == bookId, ct);
        if (!already)
        {
            _context.WishlistItems.Add(new WishlistItem { UserId = userId.Value, SaleBookId = bookId });
            await _context.SaveChangesAsync(ct);
        }
        return Ok(new { message = "Qo'shildi" });
    }

    // ======== DELETE — olib tashlash ========
    [HttpDelete("{bookId:int}")]
    public async Task<IActionResult> Remove(int bookId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.SaleBookId == bookId, ct);
        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
        return NoContent();
    }
}
