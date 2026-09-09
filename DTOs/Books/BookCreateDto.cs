using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.DTOs.Books;

/// <summary>
/// Yangi kitob yaratish uchun.
/// </summary>
public class BookCreateDto
{
    [Required(ErrorMessage = "Kitob nomi kiritilishi shart")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kitob nomi 2-200 belgi")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Muallif kiritilishi shart")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Muallif 2-150 belgi")]
    public string Author { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Yil 1000-2100 oralig'ida bo'lishi kerak")]
    public int? Year { get; set; } = DateTime.UtcNow.Year;

    [StringLength(50)]
    public string Category { get; set; } = "Boshqa";

    public bool IsAvailable { get; set; } = true;

    /// <summary>Kitob muqovasi (URL yoki base64). Ixtiyoriy.</summary>
    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Kitobni yangilash uchun.
/// </summary>
public class BookUpdateDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Author { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int? Year { get; set; }

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    /// <summary>Kitob muqovasi (URL yoki base64). Ixtiyoriy.</summary>
    public string ImageUrl { get; set; } = string.Empty;
}
