using System.ComponentModel.DataAnnotations;
using KutubxonaAPI.Models.Enums;

namespace KutubxonaAPI.Models;

/// <summary>
/// Foydalanuvchining buyurtmasi.
/// </summary>
public class Order : ISoftDelete
{
    // ===== Soft Delete =====
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }


    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public decimal TotalAmount { get; set; }

    /// <summary>Buyurtma holati (Pending, Paid, Shipped, Delivered, Cancelled).</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required]
    [StringLength(150)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}