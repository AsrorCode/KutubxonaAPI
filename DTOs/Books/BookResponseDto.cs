namespace KutubxonaAPI.DTOs.Books;

/// <summary>
/// Kitob asosiy ma'lumoti.
/// </summary>
public class BookResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Kitob + statistika (izohlar soni, o'rtacha reyting, sahifalar).
/// Bosh sahifada ishlatiladi — N+1 muammosini yechadi.
/// </summary>
public class BookWithStatsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }

    public int TotalPages { get; set; }
    public int CommentsCount { get; set; }
    public double AverageRating { get; set; }
}
