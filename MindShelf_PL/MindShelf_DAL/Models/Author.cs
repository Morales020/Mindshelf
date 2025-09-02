using System.ComponentModel.DataAnnotations;

namespace MindShelf_DAL.Models;

public class Author
{
    [Key]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Author Name Must Be Entered")]
    [MaxLength(50, ErrorMessage = "Must be 1-50 characters")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Bio must be 0-500 characters")]
    public string? Bio { get; set; }

    [MaxLength(200, ErrorMessage = "Image URL must be 0-200 characters")]
    [Url(ErrorMessage = "Image URL must be a valid URL")]
    public string? ImageUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}