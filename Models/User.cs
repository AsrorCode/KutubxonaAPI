using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Foydalanuvchi modeli - ro'yxatdan o'tgan har bir kishi.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Foydalanuvchining email manzili (unique).</summary>
    [Required(ErrorMessage = "Email kiritilishi shart")]
    [EmailAddress(ErrorMessage = "To'g'ri email kiriting")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Parol HASH - hech qachon ochiq parol saqlanmaydi!</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Foydalanuvchining ismi.</summary>
    [Required(ErrorMessage = "Ism kiritilishi shart")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Familiya.</summary>
    [Required(ErrorMessage = "Familiya kiritilishi shart")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Foydalanuvchi roli: "User", "Admin", "Seller".
    /// Default - "User".
    /// </summary>
    [StringLength(50)]
    public string Role { get; set; } = "User";

    /// <summary>Ro'yxatdan o'tgan vaqti.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Oxirgi marta qachon kirgan.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Foydalanuvchining to'liq ismi (yordamchi property).</summary>
    public string FullName => $"{FirstName} {LastName}";
}