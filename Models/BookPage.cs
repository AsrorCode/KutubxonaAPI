using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Kitobning bitta sahifasini ifodalaydi.
/// Bir nechta BookPage bitta Book ga tegishli bo'ladi (One-to-Many).
/// </summary>
public class BookPage
{
    /// <summary>
    /// Sahifaning unique ID si.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kitobdagi sahifa raqami (1, 2, 3, ...).
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Sahifa raqami 1 dan kichik bo'lmasin")]
    public int PageNumber { get; set; }

    /// <summary>
    /// Sahifa matni.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    // ============================================
    // RELATIONSHIP (BOG'LANISH)
    // ============================================

    /// <summary>
    /// Foreign Key - qaysi kitobga tegishli.
    /// EF Core "BookId" nomini ko'rib avtomatik foreign key sifatida tushunadi.
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Navigation Property - bu sahifa tegishli bo'lgan Book obyekti.
    /// "?" - chunki yuklanmasligi mumkin (Include qilmasak).
    /// </summary>
    public Book? Book { get; set; }
}