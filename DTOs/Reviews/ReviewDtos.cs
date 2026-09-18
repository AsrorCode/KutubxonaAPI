using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.DTOs.Reviews;

/// <summary>Sharh — javob (o'qish uchun).</summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
}

/// <summary>Sharh qo'shish uchun. AuthorName JWT'dan olinadi.</summary>
public class CreateReviewDto
{
    [Required(ErrorMessage = "Sharh matni shart")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Sharh 3-1000 belgi bo'lishi kerak")]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Reyting 1 va 5 oralig'ida bo'lishi kerak")]
    public int Rating { get; set; }
}

/// <summary>Kitobning barcha sharhlari + o'rtacha reyting.</summary>
public class ReviewsListDto
{
    public int SaleBookId { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public List<ReviewResponseDto> Reviews { get; set; } = new();
}
