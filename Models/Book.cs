using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Kitob modeli - ma'lumotlar bazasidagi "Books" jadvalini ifodalaydi.
/// Har bir property jadval ustuniga aylanadi.
/// </summary>
public class Book
{
    /// <summary>
    /// Kitobning unique ID raqami (Primary Key).
    /// EF Core avtomatik ravishda auto-increment qiladi.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kitob nomi - majburiy maydon.
    /// </summary>
    [Required(ErrorMessage = "Kitob nomi kiritilishi shart")]
    [StringLength(200, ErrorMessage = "Kitob nomi 200 ta belgidan oshmasligi kerak")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Muallif ismi - majburiy maydon.
    /// </summary>
    [Required(ErrorMessage = "Muallif kiritilishi shart")]
    [StringLength(150, ErrorMessage = "Muallif ismi 150 ta belgidan oshmasligi kerak")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Nashr yili - ixtiyoriy.
    /// </summary>
    [Range(1000, 2100, ErrorMessage = "Yil 1000 va 2100 oralig'ida bo'lishi kerak")]
    public int? Year { get; set; }

    /// <summary>
    /// Kategoriya (Roman, Hikoya, Ilmiy, va h.k.).
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Boshqa";

    /// <summary>
    /// Kitob holati: true = mavjud, false = olingan.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Yozuv qachon yaratilgani.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Oxirgi marta qachon o'zgartirilgani.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ============================================
    // RELATIONSHIP (BOG'LANISH)
    // ============================================

    /// <summary>
    /// Bu kitobning barcha sahifalari 
    /// Navigation property - One-to-Many bog'lanish.
    /// </summary>
    public List<BookPage> Pages { get; set; } = new();
    /// <summary>
    /// Bu kitobga qoldirilgan barcha izohlar.
    /// </summary>
    public List<Comment> Comments { get; set; } = new();
}
