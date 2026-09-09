namespace KutubxonaAPI.Common.Constants;

/// <summary>
/// Autentifikatsiya va xavfsizlik konstantalari.
/// </summary>
public static class AuthConstants
{
    /// <summary>Access token muddati (daqiqa).</summary>
    public const int AccessTokenMinutes = 15;

    /// <summary>Refresh token muddati (kun).</summary>
    public const int RefreshTokenDays = 7;

    /// <summary>Rate limit — auth endpoint'lar uchun 1 daqiqada nechta.</summary>
    public const int AuthRateLimitPerMinute = 5;

    /// <summary>Rate limit — umumiy API 1 daqiqada nechta.</summary>
    public const int GeneralRateLimitPerMinute = 100;

    /// <summary>Parol uchun minimal uzunlik.</summary>
    public const int MinPasswordLength = 8;

    /// <summary>Parol uchun maksimal uzunlik.</summary>
    public const int MaxPasswordLength = 100;

    /// <summary>Refresh token uzunligi (bayt).</summary>
    public const int RefreshTokenBytes = 64;

    /// <summary>Login urinishi limitiga rate policy nomi.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Umumiy so'rovlar uchun rate policy nomi.</summary>
    public const string GeneralRateLimitPolicy = "general";

    /// <summary>CORS policy — development muhitida.</summary>
    public const string DevelopmentCorsPolicy = "Development";

    /// <summary>CORS policy — production muhitida.</summary>
    public const string ProductionCorsPolicy = "Production";
}
