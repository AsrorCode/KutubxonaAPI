using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutubxonaAPI.Models;

/// <summary>
/// Buyurtmaning bir elementi. Order va SaleBook orasidagi bog'lanish.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    // Order bilan bog'lanish
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // SaleBook bilan bog'lanish
    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    /// <summary>Nechta sotib olindi</summary>
    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;

    /// <summary>Sotib olish paytidagi narx (keyin o'zgarsa ham buyurtma bu narxda)</summary>
    public decimal PriceAtOrder { get; set; }

    /// <summary>Umumiy: Quantity * PriceAtOrder</summary>
    public decimal Subtotal => Quantity * PriceAtOrder;
}