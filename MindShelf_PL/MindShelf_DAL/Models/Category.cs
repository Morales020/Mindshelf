using System.ComponentModel.DataAnnotations;

namespace MindShelf_DAL.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Name Must Be Required")]
    [MaxLength(100, ErrorMessage = "Must be Between 0 - 100")]
    public string Name { get; set; }

    [MaxLength(500, ErrorMessage = "Must be Between 0 - 500")]
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}