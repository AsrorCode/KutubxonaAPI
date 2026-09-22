using System.ComponentModel.DataAnnotations.Schema;

namespace KutubxonaAPI.Models;

/// <summary>
/// Kitobning qo'shimcha galereya rasmi (bir kitobda bir nechta rasm).
/// SortOrder bo'yicha tartiblanadi. Asosiy rasm — SaleBook.ImageUrl.
/// </summary>
public class SaleBookImage
{
    public int Id { get; set; }

    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Url { get; set; } = string.Empty;

    /// <summary>Chapdagi thumbnail qatoridagi tartib (0 — birinchi).</summary>
    public int SortOrder { get; set; } = 0;
}
