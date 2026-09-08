namespace KutubxonaAPI.Models;

/// <summary>
/// Soft Delete uchun interface.
/// Model'ni fizik o'chirmaymiz — faqat "o'chirilgan" deb belgilaymiz.
/// Bu tiklash imkonini beradi va audit trail'ni saqlab qoladi.
/// </summary>
public interface ISoftDelete
{
    /// <summary>O'chirilganmi.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Qachon o'chirilgan.</summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>Kim o'chirgan (user ID).</summary>
    int? DeletedByUserId { get; set; }
}
