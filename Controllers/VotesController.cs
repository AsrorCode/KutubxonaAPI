using KutubxonaAPI.Common.Extensions;
using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// "Ovoz bering" — kitobning o'qish holati bo'yicha ovozlar.
/// Turlar: read (o'qidim), reading (o'qiyapman),
/// want (o'qimoqchiman), recommend (tavsiya qilaman).
/// </summary>
[ApiController]
[Route("api/salebooks/{saleBookId:int}/votes")]
[Produces("application/json")]
public class VotesController : ControllerBase
{
    private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase) { "read", "reading", "want", "recommend" };

    private readonly AppDbContext _context;

    public VotesController(AppDbContext context)
    {
        _context = context;
    }

    public class VoteRequest
    {
        public string VoteType { get; set; } = string.Empty;
    }

    // ======== GET — ovoz sonlari + userning tanlovi ========
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetVotes(int saleBookId, CancellationToken ct = default)
    {
        var grouped = await _context.BookVotes
            .Where(v => v.SaleBookId == saleBookId)
            .GroupBy(v => v.VoteType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var counts = new Dictionary<string, int>
        {
            ["read"] = 0, ["reading"] = 0, ["want"] = 0, ["recommend"] = 0
        };
        foreach (var g in grouped)
            if (counts.ContainsKey(g.Type)) counts[g.Type] = g.Count;

        string? myVote = null;
        var userId = User.GetUserId();
        if (userId is not null)
        {
            myVote = await _context.BookVotes
                .Where(v => v.SaleBookId == saleBookId && v.UserId == userId)
                .Select(v => v.VoteType)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(new { counts, total = counts.Values.Sum(), myVote });
    }

    // ======== POST — ovoz berish / o'zgartirish / bekor qilish ========
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Vote(int saleBookId, [FromBody] VoteRequest req, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var type = (req.VoteType ?? "").Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(type))
            return BadRequest(new { message = "Noto'g'ri ovoz turi" });

        var bookExists = await _context.SaleBooks.AnyAsync(b => b.Id == saleBookId, ct);
        if (!bookExists) return NotFound(new { message = "Kitob topilmadi" });

        var existing = await _context.BookVotes
            .FirstOrDefaultAsync(v => v.SaleBookId == saleBookId && v.UserId == userId, ct);

        if (existing == null)
        {
            _context.BookVotes.Add(new BookVote
            {
                UserId = userId.Value,
                SaleBookId = saleBookId,
                VoteType = type
            });
        }
        else if (existing.VoteType == type)
        {
            // Bir xil tugma qayta bosildi — ovozni bekor qilish (toggle off)
            _context.BookVotes.Remove(existing);
        }
        else
        {
            existing.VoteType = type; // boshqa holatga o'zgartirish
        }

        await _context.SaveChangesAsync(ct);
        return await GetVotes(saleBookId, ct);
    }
}
