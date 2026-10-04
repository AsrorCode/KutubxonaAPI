using System.Text.Json;
using KutubxonaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Data;

/// <summary>
/// Bir martalik seeder — seed/salebooks.json mavjud bo'lsa va SaleBooks jadvali
/// BO'SH bo'lsa, eski kitoblarni yuklaydi (SQL Server → Postgres ko'chirish uchun).
/// Baza to'lgach, qayta ishlamaydi (idempotent).
/// </summary>
public static class BookJsonSeeder
{
    public static async Task SeedAsync(AppDbContext db, string contentRoot, ILogger logger)
    {
        var path = Path.Combine(contentRoot, "seed", "salebooks.json");
        if (!File.Exists(path)) return;

        // Faqat bo'sh bazaga yuklaymiz
        if (await db.SaleBooks.IgnoreQueryFilters().AnyAsync()) return;

        List<Dictionary<string, JsonElement>>? rows;
        try
        {
            var json = await File.ReadAllTextAsync(path);
            rows = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "salebooks.json o'qib bo'lmadi");
            return;
        }
        if (rows == null || rows.Count == 0) return;

        foreach (var r in rows)
        {
            db.SaleBooks.Add(new SaleBook
            {
                Title = Str(r, "Title"),
                Author = Str(r, "Author"),
                Description = Str(r, "Description"),
                Price = Dec(r, "Price"),
                Stock = Int(r, "Stock") ?? 0,
                ImageUrl = Str(r, "ImageUrl"),
                Category = Str(r, "Category", "Boshqa"),
                Year = Int(r, "Year"),
                Publisher = Str(r, "Publisher"),
                CoverType = Str(r, "CoverType"),
                PageCount = Int(r, "PageCount"),
                Isbn = Str(r, "Isbn"),
                ViewCount = Int(r, "ViewCount") ?? 0,
                IsActive = Bool(r, "IsActive", true),
                Discount = Int(r, "Discount") ?? 0,
                DiscountEndsAt = Date(r, "DiscountEndsAt"),
                CreatedAt = Date(r, "CreatedAt") ?? DateTime.UtcNow,
                UpdatedAt = Date(r, "UpdatedAt")
            });
        }

        await db.SaveChangesAsync();
        logger.LogInformation("📚 {Count} ta kitob JSON'dan Postgres'ga yuklandi", rows.Count);
    }

    // ===== Yordamchi o'qigichlar (JsonElement'ni xavfsiz o'qiydi) =====
    private static bool Has(Dictionary<string, JsonElement> r, string k) =>
        r.TryGetValue(k, out var v) && v.ValueKind != JsonValueKind.Null && v.ValueKind != JsonValueKind.Undefined;

    private static string Str(Dictionary<string, JsonElement> r, string k, string fallback = "") =>
        Has(r, k) && r[k].ValueKind == JsonValueKind.String ? (r[k].GetString() ?? fallback) : fallback;

    private static decimal Dec(Dictionary<string, JsonElement> r, string k) =>
        Has(r, k) && r[k].ValueKind == JsonValueKind.Number && r[k].TryGetDecimal(out var d) ? d : 0m;

    private static int? Int(Dictionary<string, JsonElement> r, string k) =>
        Has(r, k) && r[k].ValueKind == JsonValueKind.Number && r[k].TryGetInt32(out var i) ? i : null;

    private static bool Bool(Dictionary<string, JsonElement> r, string k, bool fallback) =>
        Has(r, k) && (r[k].ValueKind == JsonValueKind.True || r[k].ValueKind == JsonValueKind.False)
            ? r[k].GetBoolean() : fallback;

    private static DateTime? Date(Dictionary<string, JsonElement> r, string k) =>
        Has(r, k) && r[k].ValueKind == JsonValueKind.String && r[k].TryGetDateTime(out var dt) ? dt : null;
}
