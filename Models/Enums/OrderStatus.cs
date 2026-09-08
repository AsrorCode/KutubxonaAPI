namespace KutubxonaAPI.Models.Enums;

/// <summary>
/// Buyurtma holati.
/// </summary>
public enum OrderStatus
{
    /// <summary>Kutilmoqda — yangi buyurtma, hali to'lanmagan.</summary>
    Pending = 0,

    /// <summary>To'langan.</summary>
    Paid = 1,

    /// <summary>Jo'natilgan (yetkazuvchida).</summary>
    Shipped = 2,

    /// <summary>Yetkazilgan (buyurtma tugallandi).</summary>
    Delivered = 3,

    /// <summary>Bekor qilingan.</summary>
    Cancelled = 4
}
