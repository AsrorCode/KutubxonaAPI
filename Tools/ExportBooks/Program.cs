using System.Text.Json;
using Microsoft.Data.SqlClient;

// ============================================================
// Bir martalik vosita: eski SQL Server (LocalDB) bazasidan
// SaleBooks jadvalini o'qib, seed/salebooks.json fayliga yozadi.
// Keyin asosiy dastur ishga tushganda Postgres'ga avtomatik yuklaydi.
//
// Ishga tushirish (repo ildizidan):
//   dotnet run --project Tools/ExportBooks
// ============================================================

// Eski baza — kerak bo'lsa nomini/serverni shu yerда o'zgartiring
const string connectionString =
    "Server=(localdb)\\MSSQLLocalDB;Database=KutubxonaDB;Trusted_Connection=True;TrustServerCertificate=True";

// Faqat o'chirilmagan, faol yoki nofaol barcha kitoblar (soft-delete tashqari)
const string sql = @"
SELECT Title, Author, Description, Price, Stock, ImageUrl, Category, Year,
       Publisher, CoverType, PageCount, Isbn, ViewCount, IsActive,
       Discount, DiscountEndsAt, CreatedAt, UpdatedAt
FROM SaleBooks
WHERE IsDeleted = 0";

var books = new List<Dictionary<string, object?>>();

try
{
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    await using var cmd = new SqlCommand(sql, conn);
    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        var row = new Dictionary<string, object?>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            row[name] = reader.IsDBNull(i) ? null : reader.GetValue(i);
        }
        books.Add(row);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Eski bazaga ulanib bo'lmadi: " + ex.Message);
    Console.ResetColor();
    Console.WriteLine("Connection string'ni (server/baza nomi) tekshiring: Tools/ExportBooks/Program.cs");
    return 1;
}

// seed/ papkasiga yozamiz (repo ildizidan ishga tushirilsa)
var seedDir = Path.Combine(Directory.GetCurrentDirectory(), "seed");
Directory.CreateDirectory(seedDir);
var outPath = Path.Combine(seedDir, "salebooks.json");

var json = JsonSerializer.Serialize(books, new JsonSerializerOptions
{
    WriteIndented = true
});
await File.WriteAllTextAsync(outPath, json);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"✅ {books.Count} ta kitob eksport qilindi → {outPath}");
Console.ResetColor();
Console.WriteLine("Endi asosiy dasturni ishga tushiring (dotnet run) — Postgres bo'sh bo'lsa avtomatik yuklanadi.");
return 0;
