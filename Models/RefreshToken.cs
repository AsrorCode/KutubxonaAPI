using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Refresh Token — access token muddati o'tganida yangi olish uchun.
/// DB'da saqlanadi, revoke qilinishi mumkin.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>Token qiymati (256-bit random string).</summary>
    [Required]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>Kim ega — foydalanuvchi.</summary>
    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Qachon amal qilishi tugaydi.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Yaratilgan vaqti.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Bekor qilinganmi? (logout yoki xavfsizlik).</summary>
    public bool IsRevoked { get; set; }

    /// <summary>Qachon bekor qilingan.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Bekor qilinish sababi.</summary>
    [StringLength(100)]
    public string? RevokedReason { get; set; }

    /// <summary>Qaysi qurilma / IP dan yaratilgan (audit uchun).</summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    // Yordamchi property'lar
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
