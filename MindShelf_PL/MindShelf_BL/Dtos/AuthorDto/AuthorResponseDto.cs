using MindShelf_BL.Dtos.BookDto;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.AuthorDto
{
    public class AuthorResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public ICollection<BookResponseDto> BooksResponseDto { get; set; }
    }
}
