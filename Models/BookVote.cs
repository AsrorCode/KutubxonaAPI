using System.ComponentModel.DataAnnotations;

namespace KutubxonaAPI.Models;

/// <summary>
/// Foydalanuvchining kitobga bergan "ovozi" (o'qish holati).
/// VoteType: "read" (o'qidim), "reading" (o'qiyapman),
/// "want" (o'qimoqchiman), "recommend" (tavsiya qilaman).
/// Har (UserId, SaleBookId) juftligi unikal — bir user 1 ovoz.
/// </summary>
public class BookVote
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int SaleBookId { get; set; }
    public SaleBook? SaleBook { get; set; }

    [Required]
    [StringLength(20)]
    public string VoteType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
