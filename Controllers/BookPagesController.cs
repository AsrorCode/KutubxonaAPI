using KutubxonaAPI.Data;
using KutubxonaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace KutubxonaAPI.Controllers;

[ApiController]
[Route("api/books/{bookId}/pages")]
public class BookPagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookPagesController> _logger;

    public BookPagesController(AppDbContext context, ILogger<BookPagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================
    // GET: /api/books/{bookId}/pages
    // Barcha sahifalar ro'yxati (kichik ma'lumot)
    // ============================================
    [HttpGet]
    public async Task<IActionResult> GetPages(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        var totalPages = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .CountAsync();

        return Ok(new
        {
            bookId = book.Id,
            bookTitle = book.Title,
            bookAuthor = book.Author,
            totalPages
        });
    }

    // ============================================
    // GET: /api/books/{bookId}/pages/{pageNumber}
    // Bitta sahifa — to'liq matn
    // ============================================
    [HttpGet("{pageNumber}")]
    public async Task<IActionResult> GetPage(int bookId, int pageNumber)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        var totalPages = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .CountAsync();

        if (totalPages == 0)
            return NotFound(new { message = "Bu kitobda sahifalar yo'q" });

        if (pageNumber < 1 || pageNumber > totalPages)
            return BadRequest(new { message = $"Sahifa raqami 1 dan {totalPages} gacha bo'lishi kerak" });

        var page = await _context.BookPages
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
            return NotFound(new { message = "Sahifa topilmadi" });

        return Ok(new
        {
            id = page.Id,
            bookId = book.Id,
            bookTitle = book.Title,
            bookAuthor = book.Author,
            pageNumber = page.PageNumber,
            content = page.Content,
            totalPages,
            hasNextPage = pageNumber < totalPages,
            hasPreviousPage = pageNumber > 1
        });
    }

    // ============================================
    // POST: /api/books/{bookId}/pages
    // Bitta sahifa qo'shish (ADMIN)
    // ============================================
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePage(int bookId, [FromBody] CreatePageDto dto)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "Sahifa matni bo'sh bo'lmasligi kerak" });

        // Keyingi sahifa raqami
        var lastPageNumber = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .MaxAsync(p => (int?)p.PageNumber) ?? 0;

        var page = new BookPage
        {
            BookId = bookId,
            PageNumber = dto.PageNumber ?? lastPageNumber + 1,
            Content = dto.Content
        };

        _context.BookPages.Add(page);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Sahifa qo'shildi: kitob {BookId}, sahifa {PageNumber}",
            bookId, page.PageNumber);

        return CreatedAtAction(
            nameof(GetPage),
            new { bookId, pageNumber = page.PageNumber },
            new { page.Id, page.PageNumber, page.Content });
    }

    // ============================================
    // POST: /api/books/{bookId}/pages/bulk
    // Ko'p sahifa birdaniga (PDF matnidan) (ADMIN)
    // ============================================
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePagesBulk(int bookId, [FromBody] BulkPagesDto dto)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            return NotFound(new { message = "Kitob topilmadi" });

        if (string.IsNullOrWhiteSpace(dto.FullText))
            return BadRequest(new { message = "Matn bo'sh" });

        // Matnni chunk'larga bo'lish
        var chunks = SplitIntoChunks(dto.FullText, dto.ChunkSize ?? 2500);

        if (chunks.Count == 0)
            return BadRequest(new { message = "Matndan sahifalar ajratib bo'lmadi" });

        // Oldingi sahifalar bo'lsa — davom ettiramiz
        var lastPageNumber = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .MaxAsync(p => (int?)p.PageNumber) ?? 0;

        var pages = chunks.Select((chunk, idx) => new BookPage
        {
            BookId = bookId,
            PageNumber = lastPageNumber + idx + 1,
            Content = chunk
        }).ToList();

        _context.BookPages.AddRange(pages);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Bulk sahifalar qo'shildi: kitob {BookId}, {Count} ta sahifa",
            bookId, pages.Count);

        return Ok(new
        {
            message = $"{pages.Count} ta sahifa yaratildi",
            count = pages.Count,
            totalPages = lastPageNumber + pages.Count
        });
    }

    // ============================================
    // PUT: /api/books/{bookId}/pages/{pageNumber}
    // Sahifani yangilash (ADMIN)
    // ============================================
    [HttpPut("{pageNumber}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePage(int bookId, int pageNumber, [FromBody] UpdatePageDto dto)
    {
        var page = await _context.BookPages
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
            return NotFound(new { message = "Sahifa topilmadi" });

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "Sahifa matni bo'sh bo'lmasligi kerak" });

        page.Content = dto.Content;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Sahifa yangilandi: kitob {BookId}, sahifa {PageNumber}",
            bookId, pageNumber);

        return NoContent();
    }

    // ============================================
    // DELETE: /api/books/{bookId}/pages/{pageNumber}
    // Bitta sahifani o'chirish (ADMIN)
    // ============================================
    [HttpDelete("{pageNumber}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePage(int bookId, int pageNumber)
    {
        var page = await _context.BookPages
            .FirstOrDefaultAsync(p => p.BookId == bookId && p.PageNumber == pageNumber);

        if (page == null)
            return NotFound(new { message = "Sahifa topilmadi" });

        _context.BookPages.Remove(page);

        // Undan keyingi sahifalarning raqamini bir kamaytirish
        var subsequentPages = await _context.BookPages
            .Where(p => p.BookId == bookId && p.PageNumber > pageNumber)
            .ToListAsync();

        foreach (var p in subsequentPages)
            p.PageNumber--;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Sahifa o'chirildi: kitob {BookId}, sahifa {PageNumber}",
            bookId, pageNumber);

        return NoContent();
    }

    // ============================================
    // DELETE: /api/books/{bookId}/pages
    // Barcha sahifalarni o'chirish (ADMIN) — qayta yuklash uchun
    // ============================================
    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllPages(int bookId)
    {
        var pages = await _context.BookPages
            .Where(p => p.BookId == bookId)
            .ToListAsync();

        if (pages.Count == 0)
            return NotFound(new { message = "O'chirish uchun sahifalar yo'q" });

        _context.BookPages.RemoveRange(pages);
        await _context.SaveChangesAsync();

        _logger.LogWarning("BARCHA sahifalar o'chirildi: kitob {BookId}, {Count} ta",
            bookId, pages.Count);

        return Ok(new { message = $"{pages.Count} ta sahifa o'chirildi" });
    }

    // ============================================
    // HELPER: Matnni chunk'larga bo'lish (yaxshilangan)
    // ============================================
    private static List<string> SplitIntoChunks(string text, int chunkSize = 2500)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // 1. Tozalash — ortiqcha bo'sh joylarni olib tashlash
        text = text.Replace("\r\n", "\n").Trim();

        // 2. Paragraflar bo'yicha bo'lish (bo'sh qatorlar orqali)
        var paragraphs = text
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        if (paragraphs.Count == 0)
            return chunks;

        var currentChunk = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            // Agar bitta paragraf o'zi chunkSize dan katta bo'lsa —
            // jumla bo'yicha bo'lamiz
            if (paragraph.Length > chunkSize)
            {
                // Avval joriy chunk'ni saqlab qo'yamiz
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }

                // Katta paragrafni jumlalar bo'yicha bo'lamiz
                var sentences = SplitBySentences(paragraph);
                foreach (var sentence in sentences)
                {
                    if (currentChunk.Length + sentence.Length > chunkSize
                        && currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }
                    if (currentChunk.Length > 0)
                        currentChunk.Append(' ');
                    currentChunk.Append(sentence);
                }
                continue;
            }

            // Odatiy hol — paragraf sig'adimi tekshiramiz
            if (currentChunk.Length + paragraph.Length + 2 > chunkSize
                && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            if (currentChunk.Length > 0)
                currentChunk.Append("\n\n");
            currentChunk.Append(paragraph);
        }

        // Oxirgi chunk
        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }

    // Katta paragrafni jumlalar bo'yicha bo'lish
    private static List<string> SplitBySentences(string paragraph)
    {
        var sentences = new List<string>();
        var current = new StringBuilder();

        for (int i = 0; i < paragraph.Length; i++)
        {
            current.Append(paragraph[i]);

            // Jumla oxiri
            if ((paragraph[i] == '.' || paragraph[i] == '!' || paragraph[i] == '?')
                && (i == paragraph.Length - 1 || char.IsWhiteSpace(paragraph[i + 1])))
            {
                sentences.Add(current.ToString().Trim());
                current.Clear();
            }
        }

        if (current.Length > 0)
            sentences.Add(current.ToString().Trim());

        return sentences.Where(s => !string.IsNullOrEmpty(s)).ToList();
    }
}

// ============================================
// DTO SINFLARI
// ============================================

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
    public int? ChunkSize { get; set; }  // Ixtiyoriy — default 2500
}