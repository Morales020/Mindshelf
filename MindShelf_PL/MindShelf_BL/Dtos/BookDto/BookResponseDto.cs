using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.BookDto
{
    public class BookResponseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ReviewCount { get; set; }
        public double AvrageRating { get; set; }
        public DateTime PublishedDate { get; set; }
        public BookState BookStatus { get; set; } 


    }
}
