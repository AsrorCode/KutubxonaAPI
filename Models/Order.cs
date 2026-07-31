using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Foydalanuvchining buyurtmasi.
/// </summary>
public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public decimal TotalAmount { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Pending";

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