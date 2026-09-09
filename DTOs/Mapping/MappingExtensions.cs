using KutubxonaAPI.DTOs.Books;
using KutubxonaAPI.DTOs.Comments;
using KutubxonaAPI.DTOs.Orders;
using KutubxonaAPI.DTOs.SaleBooks;
using KutubxonaAPI.DTOs.Users;
using KutubxonaAPI.Models;

namespace KutubxonaAPI.DTOs.Mapping;

/// <summary>
/// Entity → DTO mapping extension'lari.
/// AutoMapper o'rniga qo'lda — kichik loyihada tezroq va aniq.
/// </summary>
public static class MappingExtensions
{
    // ===== USER =====
    public static UserResponseDto ToDto(this User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        FullName = u.FullName,
        Role = u.Role,
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt
    };

    // ===== BOOK =====
    public static BookResponseDto ToDto(this Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        Year = b.Year,
        Category = b.Category,
        IsAvailable = b.IsAvailable,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };

    // ===== COMMENT =====
    public static CommentResponseDto ToDto(this Comment c) => new()
    {
        Id = c.Id,
        AuthorName = c.AuthorName,
        Content = c.Content,
        Rating = c.Rating,
        CreatedAt = c.CreatedAt,
        UserId = c.UserId
    };

    // ===== SALEBOOK =====
    public static SaleBookResponseDto ToDto(this SaleBook s)
    {
        var isDiscountActive = s.Discount > 0
            && (s.DiscountEndsAt == null || s.DiscountEndsAt > DateTime.UtcNow);
        var finalPrice = isDiscountActive
            ? s.Price - (s.Price * s.Discount / 100m)
            : s.Price;

        return new()
        {
            Id = s.Id,
            Title = s.Title,
            Author = s.Author,
            Description = s.Description,
            Price = s.Price,
            Stock = s.Stock,
            ImageUrl = s.ImageUrl,
            Category = s.Category,
            Year = s.Year,
            IsActive = s.IsActive,
            Discount = isDiscountActive ? s.Discount : 0,
            DiscountEndsAt = s.DiscountEndsAt,
            FinalPrice = Math.Round(finalPrice, 0),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    // ===== ORDER ITEM =====
    public static OrderItemDto ToDto(this OrderItem i) => new()
    {
        SaleBookId = i.SaleBookId,
        BookTitle = i.SaleBook?.Title ?? string.Empty,
        BookAuthor = i.SaleBook?.Author ?? string.Empty,
        Quantity = i.Quantity,
        PriceAtOrder = i.PriceAtOrder,
        Subtotal = i.Quantity * i.PriceAtOrder
    };

    // ===== ORDER — foydalanuvchi (my orders) uchun =====
    public static OrderSummaryDto ToSummaryDto(this Order o) => new()
    {
        Id = o.Id,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ItemsCount = o.Items.Count,
        Items = o.Items.Select(i => i.ToDto()).ToList()
    };

    // ===== ORDER — admin uchun (to'liq ma'lumot) =====
    public static OrderDetailDto ToDetailDto(this Order o) => new()
    {
        Id = o.Id,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        DeliveryAddress = o.DeliveryAddress,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        ItemsCount = o.Items.Count,
        Customer = o.User == null ? null : new OrderCustomerDto
        {
            Email = o.User.Email,
            FirstName = o.User.FirstName,
            LastName = o.User.LastName
        },
        Items = o.Items.Select(i => i.ToDto()).ToList()
    };

    // ===== ORDER — yaratilganda qisqa javob =====
    public static OrderCreatedDto ToCreatedDto(this Order o) => new()
    {
        Id = o.Id,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ItemsCount = o.Items.Count
    };
}
