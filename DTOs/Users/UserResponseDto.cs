using KutubxonaAPI.Models.Enums;

namespace KutubxonaAPI.DTOs.Users;

/// <summary>
/// Foydalanuvchi ma'lumoti — API'dan qaytadi.
/// PasswordHash bu yerda YO'Q (xavfsizlik).
/// </summary>
public class UserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
