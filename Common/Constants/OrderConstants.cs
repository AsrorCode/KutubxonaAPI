namespace KutubxonaAPI.Common.Constants;

/// <summary>
/// Buyurtma bilan bog'liq konstantalar.
/// </summary>
public static class OrderConstants
{
    /// <summary>Concurrency conflict bo'lganda qayta urinishlar maksimumi.</summary>
    public const int MaxOrderRetries = 3;

    /// <summary>Retry paytida progressive backoff darajasi (ms).</summary>
    public const int RetryBackoffMs = 50;

    /// <summary>Buyurtmadagi minimum kitob soni.</summary>
    public const int MinItemsPerOrder = 1;

    /// <summary>Buyurtmadagi maksimum kitob soni (DoS oldini olish).</summary>
    public const int MaxItemsPerOrder = 100;
}
