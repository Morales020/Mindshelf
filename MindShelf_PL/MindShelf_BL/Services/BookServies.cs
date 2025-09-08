using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;

namespace MindShelf_BL.Services
{
    public class BookServies : IBookServies
    {
        private readonly UnitOfWork _UnitOfWork;

        public BookServies(UnitOfWork unitOf)
        {
            _UnitOfWork = unitOf;
        }

        #region GetAllBooks
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetAllBook(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _UnitOfWork.BookRepo
                .Query()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews);

            var totalBooks = await query.CountAsync();

            var books = await query
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
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                ImageUrl = book.ImageUrl,
                State = book.State,
                AuthorName = book.Author?.Name ?? "Unknown",
                AuthorId = book.AuthorId,
                CategoryName = book.Category?.Name ?? "Unknown",
                CategoryId = book.CategoryId,
                ReviewCount = book.Reviews?.Count ?? 0,
                Price = book.Price,
                Rating = book.Rating
            }).ToList();

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, $"Success - Total: {totalBooks}", bookDtos);
        }
        #endregion

        #region GetBookById
        public async Task<ResponseMVC<BookResponseDto>> GetBookByIdAsync(int id)
        {
            var book = await _UnitOfWork.BookRepo.Query()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.BookId == id);

            if (book == null)
                return new ResponseMVC<BookResponseDto>(404, $"Book With ID :{id} Not Found", null);

            return new ResponseMVC<BookResponseDto>(200, "Success", new BookResponseDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                ReviewCount = book.Reviews.Count,
                Price = book.Price,
                Rating = book.Rating,
                AuthorName = book.Author?.Name ?? "Unknown",
                AuthorId = book.AuthorId,
                CategoryName = book.Category?.Name ?? "Unknown",
                CategoryId = book.CategoryId,
                State = book.State,
                ImageUrl = book.ImageUrl,
            });
        }
        #endregion

        #region CreateBook
        public async Task<ResponseMVC<BookResponseDto>> CreateBookAsync(CreateBookDto createBookDto)
        {
            try
            {
                if(createBookDto== null)
                {
                    return new ResponseMVC<BookResponseDto>(400, "Unvalid payload ");
                }
                var book = new Book
                {
                    Title = createBookDto.Title,
                    Description = createBookDto.Description,
                    PublishedDate = createBookDto.PublishedDate,
                    AuthorId = createBookDto.AuthorId,
                    CategoryId = createBookDto.CategoryId,
                    Price = createBookDto.Price,
                    Rating = createBookDto.Rating,
                    ImageUrl = createBookDto.ImageUrl,
                    State = createBookDto.State
                };

                await _UnitOfWork.BookRepo.Add(book);
                await _UnitOfWork.SaveChangesAsync();

                var dto = new BookResponseDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Description = book.Description,
                    PublishedDate = book.PublishedDate,
                    Price = book.Price,
                    Rating = book.Rating,
                    State = book.State,
                    ImageUrl = book.ImageUrl
                };

                return new ResponseMVC<BookResponseDto>(201, "Book created successfully", dto);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<BookResponseDto>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region DeleteBook
        public async Task<ResponseMVC<bool>> DeleteBookAsync(int id)
        {
            var book = await _UnitOfWork.BookRepo.GetById(id);
            if (book == null)
                return new ResponseMVC<bool>(404, $"Book with ID {id} not found", false);

            _UnitOfWork.BookRepo.Delete(id);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseMVC<bool>(200, "Book deleted successfully", true);
        }
        #endregion

        #region GetBookByAuthorID
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetBooksByAuthorAsync(int authorId)
        {
            var books = await _UnitOfWork.BookRepo.Query()
                .Where(b => b.AuthorId == authorId)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            if (!books.Any())
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, $"No books found for AuthorId {authorId}", null);

            var dtos = books.Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description,
                AuthorName = b.Author?.Name ?? "Unknown",
                CategoryName = b.Category?.Name ?? "Unknown",
                ImageUrl = b.ImageUrl,
            }).ToList();

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", dtos);
        }
        #endregion

        #region GetBookByCategoryId
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetBooksByCategoryAsync(int categoryId)
        {
            var books = await _UnitOfWork.BookRepo.Query()
                .Where(b => b.CategoryId == categoryId)
                .Include(b => b.Author)
                .Include(b => b.Category)

                .ToListAsync();

            if (!books.Any())
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, $"No books found for CategoryId {categoryId}", null);

            var dtos = books.Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description,
                AuthorName = b.Author?.Name ?? "Unknown",
                CategoryName = b.Category?.Name ?? "Unknown",
                ImageUrl = b.ImageUrl,
                
            }).ToList();

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", dtos);
        }
        #endregion

        #region FilterBooks
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> FilterBooksAsync(int? categoryId, int? authorId)
        {
            try
            {
                var query = _UnitOfWork.BookRepo.Query()
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .AsQueryable();

                if (categoryId.HasValue)
                    query = query.Where(b => b.CategoryId == categoryId.Value);

                if (authorId.HasValue)
                    query = query.Where(b => b.AuthorId == authorId.Value);

                var books = await query
                    .Select(b => new BookResponseDto
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        AuthorName = b.Author != null ? b.Author.Name : null,
                        CategoryName = b.Category != null ? b.Category.Name : null,
                        Price = b.Price,
                        Rating = b.Rating,
                        Description = b.Description,
                        ImageUrl = b.ImageUrl,
                        State = b.State
                    })
                    .ToListAsync();

                return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", books);
            }
            catch (Exception ex)
            {
                return new ResponseMVC<IEnumerable<BookResponseDto>>(500, $"Error: {ex.Message}", null);
            }
        }
        #endregion

        #region NewReleases
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetNewReleasesAsync()
        {
            var books = await _UnitOfWork.BookRepo.Query()
                .OrderByDescending(b => b.PublishedDate)
                .Take(10)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            if (!books.Any())
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, "No new releases found", null);

            var dtos = books.Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description,
                AuthorName = b.Author?.Name ?? "Unknown",
                CategoryName = b.Category?.Name ?? "Unknown",
                PublishedDate = b.PublishedDate
            });

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", dtos);
        }
        #endregion

        #region SearchBooks
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> SearchBooksAsync(string searchTerm)
        {
            var books = await _UnitOfWork.BookRepo.Query()
                .Where(b => b.Title.Contains(searchTerm) || b.Description.Contains(searchTerm))
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            if (!books.Any())
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, $"هذا الكتاب غير موجود  '{searchTerm}'", null);

            var dtos = books.Select(b => new BookResponseDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description,
                AuthorName = b.Author?.Name ?? "Unknown",
                CategoryName = b.Category?.Name ?? "Unknown",
                Rating = b.Rating,
                ImageUrl=b.ImageUrl,
                 State = b.State,
                 PublishedDate = b.PublishedDate

            }).ToList();

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Success", dtos);
        }
        #endregion

        #region UpdateBook
        public async Task<ResponseMVC<BookResponseDto>> UpdateBookAsync(UpdateBookDto updateBookDto)
        {
            var book = await _UnitOfWork.BookRepo.GetById(updateBookDto.BookId);
            if (book == null)
                return new ResponseMVC<BookResponseDto>(404, $"Book with ID {updateBookDto.BookId} not found", null);

            book.Title = updateBookDto.Title;
            book.Description = updateBookDto.Description;
            book.Price = updateBookDto.Price;
            book.ImageUrl = updateBookDto.ImageUrl;
            book.State = updateBookDto.State;
            book.PublishedDate = updateBookDto.PublishedDate;

            _UnitOfWork.BookRepo.Update(book);
            await _UnitOfWork.SaveChangesAsync();

            var dto = new BookResponseDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price,
                State = book.State,
                ImageUrl = book.ImageUrl
            };

            return new ResponseMVC<BookResponseDto>(200, "Book updated successfully", dto);
        }
        #endregion

        #region GetTopRatedBooks
        public async Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetTopRatedBooksAsync(int count = 4)
        {
            var books = await _UnitOfWork.BookRepo
                .Query()
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Reviews)
                .Where(b => b.Rating > 0) // Only books with ratings
                .OrderByDescending(b => b.Rating)
                .ThenByDescending(b => b.ReviewCount) // Secondary sort by review count
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

            if (books.Count == 0)
            {
                return new ResponseMVC<IEnumerable<BookResponseDto>>(404, "No top-rated books found", null);
            }

            var bookDtos = books.Select(book => new BookResponseDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                ImageUrl = book.ImageUrl,
                Price = book.Price,
                Rating = book.Rating,
                ReviewCount = book.ReviewCount,
                State = book.State,
                AuthorName = book.Author?.Name ?? "Unknown Author",
                AuthorId = book.AuthorId,
                CategoryName = book.Category?.Name ?? "Unknown Category",
                CategoryId = book.CategoryId
            });

            return new ResponseMVC<IEnumerable<BookResponseDto>>(200, "Top-rated books retrieved successfully", bookDtos);
        }
        #endregion

        #region UpdateBookRating
        public async Task<ResponseMVC<bool>> UpdateBookRatingAsync(int bookId, double averageRating, int reviewCount)
        {
            var book = await _UnitOfWork.BookRepo.GetById(bookId);
            if (book == null)
                return new ResponseMVC<bool>(404, $"Book with ID {bookId} not found", false);

            book.Rating = averageRating;
            book.ReviewCount = reviewCount;

            _UnitOfWork.BookRepo.Update(book);
            await _UnitOfWork.SaveChangesAsync();

            return new ResponseMVC<bool>(200, "Rating updated successfully", true);
        }
        #endregion
    }
}
