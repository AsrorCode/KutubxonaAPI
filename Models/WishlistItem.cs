namespace KutubxonaAPI.Models;

/// <summary>
/// Foydalanuvchining sevimli (wishlist) kitobi.
/// (UserId, SaleBookId) juftligi unikal.
/// </summary>
public class WishlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
