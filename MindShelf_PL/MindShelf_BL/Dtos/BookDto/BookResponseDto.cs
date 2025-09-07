using MindShelf_DAL.Models;
using System;

namespace MindShelf_BL.Dtos.BookDto
{
    public class BookResponseDto
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }


        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int ReviewCount { get; set; }
        public double Rating { get; set; }


        public DateTime PublishedDate { get; set; }
        public BookState State { get; set; }
    }
}
