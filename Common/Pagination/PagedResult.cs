namespace KutubxonaAPI.Common.Pagination;

/// <summary>
/// Sahifalangan natija (opt-in). ?page va ?pageSize berilганда qaytariladi.
/// page berilmasa — eski (array) javob saqlanadi, moslik buzilmaydi.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 100;

    /// <summary>pageSize ni xavfsiz chegaralaydi (1..Max).</summary>
    public static int NormalizePageSize(int? requested) =>
        Math.Clamp(requested ?? DefaultPageSize, 1, MaxPageSize);

    /// <summary>page ni xavfsiz chegaralaydi (>=1).</summary>
    public static int NormalizePage(int? requested) => Math.Max(1, requested ?? 1);
}
