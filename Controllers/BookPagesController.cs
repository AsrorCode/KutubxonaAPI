using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace KutubxonaAPI.Controllers;

[ApiController]
[Route("api/books/{bookId:int}/pages")]
[Produces("application/json")]
public class BookPagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookPagesController> _logger;

    public BookPagesController(AppDbContext context, ILogger<BookPagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPages(int bookId)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
        {
            return NotFound(new { message = $"ID = {bookId} bo'lgan kitob topilmadi" });
        }

        var pages = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .OrderBy(p => p.PageNumber)
            .Select(p => new
            {
                p.Id,
                p.PageNumber,
                ContentPreview = p.Content.Length > 100
                    ? p.Content.Substring(0, 100) + "..."
                    : p.Content
            })
            .ToListAsync();

        return Ok(new
        {
            bookId,
            totalPages = pages.Count,
            pages
        });
    }

    [HttpGet("{pageNumber:int}")]
    public async Task<IActionResult> GetPage(int bookId, int pageNumber)
    {
        var page = await _context.BookPages
            .Include(p => p.Book)
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
        {
            return NotFound(new
            {
                message = $"Kitob ID={bookId}, sahifa {pageNumber} topilmadi"
            });
        }

        var totalPages = await _context.BookPages.CountAsync(p => p.BookId == bookId);

        return Ok(new
        {
            page.Id,
            page.PageNumber,
            page.Content,
            bookId = page.BookId,
            bookTitle = page.Book?.Title,
            bookAuthor = page.Book?.Author,
            totalPages,
            hasNextPage = pageNumber < totalPages,
            hasPreviousPage = pageNumber > 1
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreatePage(int bookId, [FromBody] CreatePageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
            return NotFound(new { message = $"ID = {bookId} bo'lgan kitob topilmadi" });

        var pageNumber = dto.PageNumber ?? (await _context.BookPages
            .Where(p => p.BookId == bookId)
            .Select(p => (int?)p.PageNumber)
            .MaxAsync() ?? 0) + 1;

        var newPage = new BookPage
        {
            BookId = bookId,
            PageNumber = pageNumber,
            Content = dto.Content
        };

        _context.BookPages.Add(newPage);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yangi sahifa qo'shildi. Kitob: {BookId}, Sahifa: {PageNumber}",
            bookId, pageNumber);

        return CreatedAtAction(
            nameof(GetPage),
            new { bookId, pageNumber = newPage.PageNumber },
            newPage);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("bulk")]
    public async Task<IActionResult> CreatePagesBulk(int bookId, [FromBody] BulkPagesDto dto)
    {
        var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
        if (!bookExists)
            return NotFound(new { message = $"ID = {bookId} bo'lgan kitob topilmadi" });

        if (dto.ReplaceExisting)
        {
            var oldPages = await _context.BookPages
                .Where(p => p.BookId == bookId)
                .ToListAsync();
            _context.BookPages.RemoveRange(oldPages);
        }

        int charsPerPage = dto.CharactersPerPage > 0 ? dto.CharactersPerPage : 1500;
        var chunks = SplitIntoChunks(dto.FullText, charsPerPage);

        var newPages = new List<BookPage>();
        for (int i = 0; i < chunks.Count; i++)
        {
            newPages.Add(new BookPage
            {
                BookId = bookId,
                PageNumber = i + 1,
                Content = chunks[i]
            });
        }

        await _context.BookPages.AddRangeAsync(newPages);
        await _context.SaveChangesAsync();

        return Created($"/api/books/{bookId}/pages", new
        {
            bookId,
            totalPagesCreated = newPages.Count,
            charactersPerPage = charsPerPage
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{pageNumber:int}")]
    public async Task<IActionResult> UpdatePage(int bookId, int pageNumber, [FromBody] UpdatePageDto dto)
    {
        var page = await _context.BookPages
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
            return NotFound(new { message = "Sahifa topilmadi" });

        page.Content = dto.Content;
        await _context.SaveChangesAsync();

        return Ok(page);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{pageNumber:int}")]
    public async Task<IActionResult> DeletePage(int bookId, int pageNumber)
    {
        var page = await _context.BookPages
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
            return NotFound(new { message = "Sahifa topilmadi" });

        _context.BookPages.Remove(page);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static List<string> SplitIntoChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        if (string.IsNullOrEmpty(text)) return chunks;

        var words = text.Split(' ');
        var currentChunk = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            if (currentChunk.Length + word.Length + 1 > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }
            currentChunk.Append(word).Append(' ');
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }
}

// DTO klasslar
public class CreatePageDto
{
    public int? PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdatePageDto
{
    public string Content { get; set; } = string.Empty;
}

public class BulkPagesDto
{
    public string FullText { get; set; } = string.Empty;
    public int CharactersPerPage { get; set; } = 1500;
    public bool ReplaceExisting { get; set; } = true;
}