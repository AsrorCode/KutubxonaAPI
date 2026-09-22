using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.DTOs.SaleBooks;

public class SaleBookResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? Year { get; set; }
    public bool IsActive { get; set; }
    public int Discount { get; set; }
    public DateTime? DiscountEndsAt { get; set; }
    public decimal FinalPrice { get; set; }  // Chegirma bilan hisoblangan
    public string Publisher { get; set; } = string.Empty;
    public string CoverType { get; set; } = string.Empty;
    public int? PageCount { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    /// <summary>Qo'shimcha galereya rasmlari (asosiy rasmdan tashqari).</summary>
    public List<string> GalleryUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaleBookCreateDto
{
    [Required(ErrorMessage = "Kitob nomi shart")]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Muallif shart")]
    [StringLength(150, MinimumLength = 2)]
    public string Author { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 100_000_000, ErrorMessage = "Narx 0 dan katta bo'lishi kerak")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    // Base64 rasmlar uchun cheklov yo'q (frontend'da 2MB tekshiriladi)
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "Boshqa";

    [Range(1000, 2100)]
    public int? Year { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, 100)]
    public int Discount { get; set; } = 0;

    public DateTime? DiscountEndsAt { get; set; }

    [StringLength(150)]
    public string Publisher { get; set; } = string.Empty;

    [StringLength(50)]
    public string CoverType { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int? PageCount { get; set; }

    [StringLength(50)]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>Qo'shimcha galereya rasmlari (URL yoki data:). Maks 8 ta.</summary>
    public List<string>? GalleryUrls { get; set; }
}
