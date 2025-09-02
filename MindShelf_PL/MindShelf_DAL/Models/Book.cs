using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        [Required(ErrorMessage = "Must be Entered")]
        [MaxLength(200,ErrorMessage = "Must be Between 0 - 200")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description Is Required")]
        [MaxLength(5000, ErrorMessage = "Must be Between 0 - 5000")]
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ImageUrl { get; set; }
        public int ReviewCount { get; set; }
        public double Rating { get; set; }
        public BookState State { get; set; } = BookState.Available;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [ForeignKey(nameof(Author))]
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }

    public enum BookState
    {
        Available,
        Reserved,
        Borrowed,
        OutOfStock
    }
}
