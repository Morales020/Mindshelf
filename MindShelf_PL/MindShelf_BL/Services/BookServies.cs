using Azure;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Services
{
    public class BookServies:IBookServies
    {
        private readonly UnitOfWork _UnitOfWork;

        public BookServies( UnitOfWork unitOf)
        {
            _UnitOfWork = unitOf;
        }
        #region getallbook
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetAllBook(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var books = await _UnitOfWork.BookRepo
                .Query()
                .Include(s => s.Author)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (books.Count == 0)
            {
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, "Books not found", null);
            }

            var bookDtos = books.Select(book => new BookResponseDto
            {
                Title = book.Title,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                ImageUrl = book.ImageUrl,
                BookStatus = book.State,
                AuthorName = book.Author?.Name ?? "Unknown",
                ReviewCount = book.Reviews?.Count ?? 0,
                Price = book.Price,
                AvrageRating = book.Rating 
            }).ToList();

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", bookDtos);
        }


        #endregion
    }
}
