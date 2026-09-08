namespace KutubxonaAPI.DTOs.Auth;

/// <summary>
/// Token refresh javob.
/// </summary>
public class RefreshResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
