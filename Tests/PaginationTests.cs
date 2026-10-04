using KutubxonaAPI.Common.Pagination;
using Xunit;

namespace KutubxonaAPI.Tests;

/// <summary>
/// PagedResult yordamchi mantiq testlari.
/// </summary>
public class PaginationTests
{
    [Theory]
    [InlineData(100, 24, 5)]   // 100/24 = 4.16 → 5 sahifa
    [InlineData(48, 24, 2)]
    [InlineData(0, 24, 0)]
    public void TotalPages_IsCeiling(int total, int pageSize, int expected)
    {
        var r = new PagedResult<int> { TotalItems = total, PageSize = pageSize, Page = 1 };
        Assert.Equal(expected, r.TotalPages);
    }

    [Fact]
    public void HasNextAndPrevious_Work()
    {
        var r = new PagedResult<int> { TotalItems = 100, PageSize = 24, Page = 2 };
        Assert.True(r.HasNext);
        Assert.True(r.HasPrevious);

        var first = new PagedResult<int> { TotalItems = 100, PageSize = 24, Page = 1 };
        Assert.False(first.HasPrevious);
    }

    [Theory]
    [InlineData(null, 24)]   // default
    [InlineData(10, 10)]
    [InlineData(5000, 100)]  // Max chegaralanadi
    [InlineData(0, 1)]       // min 1
    public void NormalizePageSize_Clamps(int? input, int expected)
    {
        Assert.Equal(expected, PagedResult<int>.NormalizePageSize(input));
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void NormalizePage_MinimumOne(int? input, int expected)
    {
        Assert.Equal(expected, PagedResult<int>.NormalizePage(input));
    }
}
