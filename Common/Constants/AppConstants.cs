namespace KutubxonaAPI.Common.Constants;

/// <summary>
/// Loyiha bo'yicha umumiy konstantalar.
/// Magic numbers/strings shu yerda saqlanadi.
/// </summary>
public static class AppConstants
{
    /// <summary>Loyiha nomi (loglash, xato javoblar uchun).</summary>
    public const string AppName = "KutubxonaAPI";

    /// <summary>Standart pagination hajmi.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Maksimum pagination hajmi (DoS oldini olish).</summary>
    public const int MaxPageSize = 100;

    /// <summary>Kitob sahifasidagi standart chunk hajmi (belgi).</summary>
    public const int DefaultPageChunkSize = 3500;

    /// <summary>Foydalanuvchi rasm yuklashda maksimum hajm (2 MB).</summary>
    public const int MaxImageSizeBytes = 2 * 1024 * 1024;

    /// <summary>Standart cache muddati (sekund).</summary>
    public const int DefaultCacheSeconds = 60;
}
