using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Email tasdiqlash yoki parol tiklash uchun bir martalik token.
/// </summary>
public class UserToken
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [StringLength(200)]
    public string Token { get; set; } = string.Empty;

    /// <summary>Turi: "verify" (email tasdiqlash) yoki "reset" (parol tiklash).</summary>
    [StringLength(20)]
    public string Type { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
