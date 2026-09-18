using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Marketplace kitobiga qoldirilgan sharh va reyting.
/// Bir foydalanuvchi bitta kitobga faqat 1 marta sharh qoldiradi.
/// </summary>
public class Review : ISoftDelete
{
    // ===== Soft Delete =====
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

    public int Id { get; set; }

    /// <summary>Sharh muallifining ismi (JWT'dan avtomatik olinadi).</summary>
    [StringLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sharh matni shart")]
    [StringLength(1000, ErrorMessage = "Sharh 1000 belgidan oshmasligi kerak")]
    public string Content { get; set; } = string.Empty;

    /// <summary>Reyting — 1 dan 5 gacha yulduz.</summary>
    [Range(1, 5, ErrorMessage = "Reyting 1 va 5 oralig'ida bo'lishi kerak")]
    public int Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ===== Bog'lanishlar =====

    /// <summary>Qaysi marketplace kitobiga tegishli.</summary>
    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    /// <summary>Sharhni yozgan foydalanuvchi.</summary>
    public int UserId { get; set; }
    public User? User { get; set; }
}
