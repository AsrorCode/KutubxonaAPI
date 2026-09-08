using KutubxonaAPI.Models.Enums;

namespace KutubxonaAPI.DTOs.Orders;

/// <summary>
/// Buyurtma qisqacha (foydalanuvchi tarixi).
/// </summary>
public class OrderSummaryDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Buyurtma to'liq (admin ko'radi).
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }
    public OrderCustomerDto? Customer { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Buyurtma egasining ma'lumoti.
/// </summary>
public class OrderCustomerDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Buyurtmadagi bitta kitob.
/// </summary>
public class OrderItemDto
{
    public int SaleBookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Buyurtma yaratish javobi.
/// </summary>
public class OrderCreatedDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }
}
