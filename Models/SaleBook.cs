using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

public class SaleBook
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kitob nomi shart")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Muallif shart")]
    [StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000000, ErrorMessage = "Narx 0 dan katta bo'lishi kerak")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "Boshqa";

    [Range(1000, 2100)]
    public int? Year { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}