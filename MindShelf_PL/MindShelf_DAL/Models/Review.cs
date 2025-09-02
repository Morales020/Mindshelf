using System.ComponentModel.DataAnnotations;

namespace MindShelf_DAL.Models;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;

    [Range(0, 5)]
    public double Rating { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int BookId { get; set; }

    [Required]
    public Book Book { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public User User { get; set; }
}