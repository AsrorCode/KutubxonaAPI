namespace KutubxonaAPI.Models.Enums;

/// <summary>
/// Foydalanuvchi rollari.
/// </summary>
public enum UserRole
{
    /// <summary>Oddiy foydalanuvchi — o'qish, izoh, buyurtma.</summary>
    User = 0,

    /// <summary>Administrator — barcha huquqlar.</summary>
    Admin = 1,

    /// <summary>Sotuvchi — o'z kitoblarini boshqarish (kelajakda).</summary>
    Seller = 2
}
