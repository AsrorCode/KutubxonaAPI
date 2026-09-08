using KutubxonaAPI.DTOs.Users;

namespace KutubxonaAPI.DTOs.Auth;

/// <summary>
/// Login/Register muvaffaqiyatli javob.
/// </summary>
public class AuthResponseDto
{
    public string Message { get; set; } = string.Empty;

    /// <summary>Backward compat — eski frontend uchun.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Access token (15 daqiqa).</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Refresh token (7 kun).</summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>Access token amal qilish vaqti (sekundlarda).</summary>
    public int ExpiresIn { get; set; }

    public UserResponseDto User { get; set; } = new();
}
