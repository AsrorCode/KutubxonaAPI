using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Tematik to'plam (masalan "Yil top-100", "Biznes kitoblari").
/// Bosh sahifada alohida qator sifatida ko'rinadi.
/// </summary>
public class Collection
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nom shart")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Bosh sahifada tartib (kichik = yuqorida).</summary>
    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<CollectionItem> Items { get; set; } = new();
}

/// <summary>To'plam va kitob orasidagi bog'lanish.</summary>
public class CollectionItem
{
    public int Id { get; set; }

    public int CollectionId { get; set; }
    public Collection? Collection { get; set; }

    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    public int SortOrder { get; set; } = 0;
}
