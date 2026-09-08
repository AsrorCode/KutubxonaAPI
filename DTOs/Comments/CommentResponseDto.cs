namespace KutubxonaAPI.DTOs.Comments;

/// <summary>
/// Bitta izoh.
/// </summary>
public class CommentResponseDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
}

/// <summary>
/// Izohlar ro'yxati + o'rtacha reyting.
/// </summary>
public class CommentsListDto
{
    public int BookId { get; set; }
    public int TotalComments { get; set; }
    public double AverageRating { get; set; }
    public List<CommentResponseDto> Comments { get; set; } = new();
}
