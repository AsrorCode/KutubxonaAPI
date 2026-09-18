using KutubxonaAPI.Data;
using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Controllers;

/// <summary>
/// Tematik to'plamlar (kolleksiyalar). GET ochiq, boshqarish faqat Admin.
/// </summary>
[ApiController]
[Route("api/collections")]
[Produces("application/json")]
public class CollectionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CollectionsController(AppDbContext context)
    {
        _context = context;
    }

    // ======== GET — faol to'plamlar + kitoblari (ochiq) ========
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var collections = await _context.Collections
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Include(c => c.Items.OrderBy(i => i.SortOrder))
                .ThenInclude(i => i.SaleBook)
            .ToListAsync(ct);

        var result = collections.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            description = c.Description,
            books = c.Items
                .Where(i => i.SaleBook != null && i.SaleBook.IsActive)
                .Select(i => i.SaleBook!.ToDto())
                .ToList()
        }).Where(c => c.books.Count > 0);

        return Ok(result);
    }

    // ======== GET /admin — barcha to'plamlar (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin(CancellationToken ct = default)
    {
        var collections = await _context.Collections
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Include(c => c.Items).ThenInclude(i => i.SaleBook)
            .ToListAsync(ct);

        var result = collections.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            description = c.Description,
            sortOrder = c.SortOrder,
            isActive = c.IsActive,
            books = c.Items
                .Where(i => i.SaleBook != null)
                .Select(i => new { id = i.SaleBook!.Id, title = i.SaleBook.Title })
                .ToList()
        });

        return Ok(result);
    }

    // ======== POST (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Collection dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Nom shart" });

        var c = new Collection
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? "",
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Collections.Add(c);
        await _context.SaveChangesAsync(ct);
        return Created($"/api/collections/{c.Id}", new { c.Id, c.Name });
    }

    // ======== PUT (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Collection dto, CancellationToken ct = default)
    {
        var c = await _context.Collections.FindAsync(new object?[] { id }, ct);
        if (c == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Name)) c.Name = dto.Name.Trim();
        c.Description = dto.Description?.Trim() ?? "";
        c.SortOrder = dto.SortOrder;
        c.IsActive = dto.IsActive;
        await _context.SaveChangesAsync(ct);
        return Ok(new { c.Id, c.Name });
    }

    // ======== DELETE (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var c = await _context.Collections.FindAsync(new object?[] { id }, ct);
        if (c == null) return NotFound();
        _context.Collections.Remove(c);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    // ======== POST — to'plamga kitob qo'shish (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/books/{bookId:int}")]
    public async Task<IActionResult> AddBook(int id, int bookId, CancellationToken ct = default)
    {
        var exists = await _context.Collections.AnyAsync(c => c.Id == id, ct);
        if (!exists) return NotFound(new { message = "To'plam topilmadi" });

        var bookExists = await _context.SaleBooks.AnyAsync(b => b.Id == bookId, ct);
        if (!bookExists) return NotFound(new { message = "Kitob topilmadi" });

        var already = await _context.CollectionItems.AnyAsync(ci => ci.CollectionId == id && ci.SaleBookId == bookId, ct);
        if (already) return Conflict(new { message = "Kitob allaqachon to'plamda" });

        _context.CollectionItems.Add(new CollectionItem { CollectionId = id, SaleBookId = bookId });
        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Qo'shildi" });
    }

    // ======== DELETE — to'plamdan kitob olib tashlash (Admin) ========
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}/books/{bookId:int}")]
    public async Task<IActionResult> RemoveBook(int id, int bookId, CancellationToken ct = default)
    {
        var item = await _context.CollectionItems
            .FirstOrDefaultAsync(ci => ci.CollectionId == id && ci.SaleBookId == bookId, ct);
        if (item == null) return NotFound();
        _context.CollectionItems.Remove(item);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
