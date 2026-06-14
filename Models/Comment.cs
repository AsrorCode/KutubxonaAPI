using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Kitobga qoldirilgan izoh (sharh).
/// Bir nechta Comment bitta Book ga tegishli (One-to-Many).
/// </summary>
public class Comment
{
    /// <summary>
    /// Izohning unique ID si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Izoh qoldirgan odamning ismi.
    /// </summary>
    [Required(ErrorMessage = "Ism kiritilishi shart")]
    [StringLength(100, ErrorMessage = "Ism 100 ta belgidan oshmasligi kerak")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Izoh matni.
    /// </summary>
    [Required(ErrorMessage = "Izoh matni kiritilishi shart")]
    [StringLength(1000, ErrorMessage = "Izoh 1000 ta belgidan oshmasligi kerak")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Reyting - 1 dan 5 gacha yulduz.
    /// </summary>
    [Range(1, 5, ErrorMessage = "Reyting 1 va 5 oralig'ida bo'lishi kerak")]
    public int Rating { get; set; }

    /// <summary>
    /// Izoh qachon qoldirilgani.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ============================================
    // RELATIONSHIP (BOG'LANISH)
    // ============================================

    /// <summary>
    /// Foreign Key - qaysi kitobga tegishli.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Navigation Property - bu izoh tegishli bo'lgan Book.
    /// </summary>
    public Book? Book { get; set; }
}