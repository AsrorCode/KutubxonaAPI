using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Sayt ichi bildirishnoma (hammaga ko'rinadigan yangilik).
/// Masalan: yangi aksiya, yangi kitob. SMS emas — sayt panelида ko'rinadi.
/// </summary>
public class Notification
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(400)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Tur: "discount", "new", "info".</summary>
    [StringLength(30)]
    public string Type { get; set; } = "info";

    /// <summary>Bog'liq kitob (ixtiyoriy) — bosilганда o'sha kitob ochiladi.</summary>
    public int? SaleBookId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
