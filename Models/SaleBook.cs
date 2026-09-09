using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutubxonaAPI.Models;

public class SaleBook : ISoftDelete
{
    // ===== Soft Delete =====
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }


    public int Id { get; set; }

    [Required(ErrorMessage = "Kitob nomi shart")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Muallif shart")]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000000, ErrorMessage = "Narx 0 dan katta bo'lishi kerak")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    // Base64 rasmlarga joy kerak — nvarchar(max)
    [Column(TypeName = "nvarchar(max)")]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "Boshqa";

    [Range(1000, 2100)]
    public int? Year { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Chegirma foizi (0-100). Ixtiyoriy — 0 bo'lsa yo'q.</summary>
    [Range(0, 100)]
    public int Discount { get; set; } = 0;

    /// <summary>Chegirma tugash sanasi. NULL bo'lsa cheklovsiz.</summary>
    public DateTime? DiscountEndsAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token — bir vaqtda 2 kishi Stock ni o'zgartira olmaydi.
    /// EF Core avtomatik boshqaradi.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}