using MindShelf_DAL.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MindShelf_BL.Dtos.BookDto
{
    public class CreateBookDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(5000)]
        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; }

      
        public int CategoryId { get; set; }

     
        public int AuthorId { get; set; }

        public BookState State { get; set; }
    }
}
