using KutubxonaAPI.DTOs.Mapping;
using KutubxonaAPI.Models;
using Xunit;

namespace KutubxonaAPI.Tests;

/// <summary>
/// SaleBook → DTO mapping va chegirma hisob-kitobi testlari (DB'siz, sof mantiq).
/// </summary>
public class MappingTests
{
    private static SaleBook Book(decimal price, int discount, DateTime? endsAt = null) => new()
    {
        Id = 1,
        Title = "Test kitob",
        Author = "Muallif",
        Price = price,
        Discount = discount,
        DiscountEndsAt = endsAt,
        Images = new List<SaleBookImage>()
    };

    [Fact]
    public void NoDiscount_FinalPriceEqualsPrice()
    {
        var dto = Book(100_000, 0).ToDto();
        Assert.Equal(100_000, dto.FinalPrice);
        Assert.Equal(0, dto.Discount);
    }

    [Fact]
    public void ActiveDiscount_ReducesFinalPrice()
    {
        var dto = Book(100_000, 20).ToDto();
        Assert.Equal(80_000, dto.FinalPrice);
        Assert.Equal(20, dto.Discount);
    }

    [Fact]
    public void ExpiredDiscount_IsIgnored()
    {
        var dto = Book(100_000, 50, DateTime.UtcNow.AddDays(-1)).ToDto();
        Assert.Equal(100_000, dto.FinalPrice);
        Assert.Equal(0, dto.Discount); // muddati o'tgan — chegirma qo'llanmaydi
    }

    [Fact]
    public void FutureDiscountEnd_IsApplied()
    {
        var dto = Book(200_000, 25, DateTime.UtcNow.AddDays(3)).ToDto();
        Assert.Equal(150_000, dto.FinalPrice);
    }

    [Fact]
    public void GalleryUrls_MappedInSortOrder()
    {
        var book = Book(50_000, 0);
        book.Images.Add(new SaleBookImage { Url = "b.jpg", SortOrder = 1 });
        book.Images.Add(new SaleBookImage { Url = "a.jpg", SortOrder = 0 });

        var dto = book.ToDto();

        Assert.Equal(new[] { "a.jpg", "b.jpg" }, dto.GalleryUrls);
    }
}
