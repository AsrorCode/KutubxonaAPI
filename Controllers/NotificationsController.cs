using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Sayt ichi bildirishnomalar (yangiliklar). Hammaga ochiq o'qish uchun.
/// Yaratish/o'chirish faqat Admin. Aksiya/yangi kitob avtomatik qo'shiladi.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    // ======== GET — so'nggi bildirishnomalar (ochiq) ========
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetAll(
        [FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (take < 1) take = 20;
        if (take > 100) take = 100;

        var items = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(ct);

        return Ok(items);
    }

    // ======== POST — qo'lda bildirishnoma (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Notification>> Create([FromBody] Notification dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Sarlavha shart" });

        var n = new Notification
        {
            Title = dto.Title.Trim(),
            Message = dto.Message?.Trim() ?? "",
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "info" : dto.Type.Trim(),
            SaleBookId = dto.SaleBookId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Notifications.Add(n);
        await _context.SaveChangesAsync(ct);
        return Created($"/api/notifications/{n.Id}", n);
    }

    // ======== DELETE (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var n = await _context.Notifications.FindAsync(new object?[] { id }, ct);
        if (n == null) return NotFound();
        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
