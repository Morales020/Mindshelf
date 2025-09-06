using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Services
{
    public class AuthorServies: IAuthorServies
    {
        private readonly UnitOfWork _UnitOfWork;
        public AuthorServies(UnitOfWork unitOf)
        {
            _UnitOfWork = unitOf;
        }

        #region getallAuthor
        public async Task<ResponseMVC<IEnumerable<AuthorResponseDto>>> GetAllAuthor(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var Authors = await _UnitOfWork.AuthorRepo
                .Query()
                .Include(s => s.Books)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (Authors.Count == 0)
            {
                return new ResponseMVC<IEnumerable<AuthorResponseDto>>(404, "Authors not found", null);
            }

            var AuthorDtos = Authors.Select(Author => new AuthorResponseDto
            {
                Name = Author.Name,
                Bio = Author.Bio,
                DateOfBirth = Author.DateOfBirth,
                ImageUrl = Author.ImageUrl,
                BooksResponseDto = Author.Books.Select(book => new BookResponseDto
                {
                    Title = book.Title,
                    Description = book.Description,
                    PublishedDate = book.PublishedDate,
                    ImageUrl = book.ImageUrl,
                    BookStatus = book.State,
                    ReviewCount = book.Reviews?.Count ?? 0,
                    Price = book.Price,
                    AvrageRating = book.Rating 
                }).ToList()
            }).ToList();

            return new ResponseMVC<IEnumerable<AuthorResponseDto>>(200, "Success", AuthorDtos);
        }


        #endregion

        #region GetAuthorById
        public async Task<ResponseMVC<AuthorResponseDto>> GetAuthorById(int id)
        {
            var author = await _UnitOfWork.AuthorRepo
                .Query()
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
                return new ResponseMVC<AuthorResponseDto>(404, "Author not found", null);

            var authorDto = new AuthorResponseDto
            {
                Name = author.Name,
                Bio = author.Bio,
                DateOfBirth = author.DateOfBirth,
                ImageUrl = author.ImageUrl,
                BooksResponseDto = author.Books.Select(book => new BookResponseDto
                {
                    Title = book.Title,
                    Description = book.Description,
                    PublishedDate = book.PublishedDate,
                    ImageUrl = book.ImageUrl,
                    BookStatus = book.State,
                    ReviewCount = book.Reviews?.Count ?? 0,
                    Price = book.Price,
                    AvrageRating = book.Rating
                }).ToList()
            };

            return new ResponseMVC<AuthorResponseDto>(200, "Success", authorDto);
        }
        #endregion

        #region CreateAuthor
        public async Task<ResponseMVC<string>> CreateAuthor(AuthorCreateDto authorDto)
        {
            var newAuthor = new Author
            {
                Name = authorDto.Name,
                Bio = authorDto.Bio,
                DateOfBirth = authorDto.DateOfBirth,
                ImageUrl = authorDto.ImageUrl
            };

            await _UnitOfWork.AuthorRepo.Add(newAuthor);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseMVC<string>(201, "Author added successfully", null);
        }
        #endregion

        #region UpdateAuthor
        public async Task<ResponseMVC<string>> UpdateAuthor(int id, AuthorUpdateDto authorDto)
        {
            var author = await _UnitOfWork.AuthorRepo.GetById(id);
            if (author == null)
                return new ResponseMVC<string>(404, "Author not found", null);

            author.Name = authorDto.Name;
            author.Bio = authorDto.Bio;
            author.DateOfBirth = authorDto.DateOfBirth;
            author.ImageUrl = authorDto.ImageUrl;

            _UnitOfWork.AuthorRepo.Update(author);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseMVC<string>(200, "Author updated successfully", null);
        }
        #endregion

        #region DeleteAuthor
        public async Task<ResponseMVC<string>> DeleteAuthor(int id)
        {
            var author = await _UnitOfWork.AuthorRepo.GetById(id);
            if (author == null)
                return new ResponseMVC<string>(404, "Author not found", null);

            await _UnitOfWork.AuthorRepo.Delete(id);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseMVC<string>(200, "Author deleted successfully", null);
        }
        #endregion

    }
}
