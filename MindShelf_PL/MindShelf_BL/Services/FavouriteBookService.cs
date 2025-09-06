using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.FavouriteBookDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindShelf_BL.Services
{
    public class FavouriteBookService : IFavouriteBookService
    {
        private readonly UnitOfWork _unitOfWork;

        public FavouriteBookService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // جلب كل المفضلة لمستخدم معين
        public async Task<ResponseMVC<IEnumerable<FavouriteBookResponseDto>>> GetAllFavouriteBooks(string userId, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(userId))
                return new ResponseMVC<IEnumerable<FavouriteBookResponseDto>>(400, "المستخدم غير محدد", null);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var favouriteBooksQuery = _unitOfWork.FavoriteBookRepo
                .Query()
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                .AsNoTracking();

            var totalCount = await favouriteBooksQuery.CountAsync();

            if (totalCount == 0)
                return new ResponseMVC<IEnumerable<FavouriteBookResponseDto>>(404, "لا توجد كتب مفضلة", null);

            var favouriteBooks = await favouriteBooksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var favouriteBookDtos = favouriteBooks.Select(f => new FavouriteBookResponseDto
            {
                FavouriteBookId = f.FavouriteBookId,
                BookId = f.BookId,
                UserId = f.UserId,
                UserName = f.UserName,
                AddedDate = f.AddedDate,
                BookTitle = f.Book?.Title,
                BookImageUrl = f.Book?.ImageUrl
            }).ToList();

            return new ResponseMVC<IEnumerable<FavouriteBookResponseDto>>(200, "نجاح", favouriteBookDtos);
        }

        // إضافة كتاب للمفضلة
        public async Task<FavouriteBookResponseDto> AddFavouriteBookAsync(string userId, int bookId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var book = await _unitOfWork._dbcontext.Books.FindAsync(bookId);
            if (book == null)
                return null;

            var existing = await _unitOfWork.FavoriteBookRepo
                .Query()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (existing != null)
            {
                return new FavouriteBookResponseDto
                {
                    FavouriteBookId = existing.FavouriteBookId,
                    BookId = existing.BookId,
                    UserId = existing.UserId,
                    UserName = existing.UserName,
                    AddedDate = existing.AddedDate,
                    BookTitle = book.Title,
                    BookImageUrl = book.ImageUrl
                };
            }

            var userName = await _unitOfWork._dbcontext.Users
                                .Where(u => u.Id == userId)
                                .Select(u => u.UserName)
                                .FirstOrDefaultAsync();

            var favouriteBook = new FavouriteBook
            {
                UserId = userId,
                BookId = bookId,
                UserName = userName,
                AddedDate = DateTime.UtcNow
            };

            await _unitOfWork.FavoriteBookRepo.Add(favouriteBook);
            await _unitOfWork.SaveChangesAsync();

            return new FavouriteBookResponseDto
            {
                FavouriteBookId = favouriteBook.FavouriteBookId,
                BookId = favouriteBook.BookId,
                UserId = favouriteBook.UserId,
                UserName = favouriteBook.UserName,
                AddedDate = favouriteBook.AddedDate,
                BookTitle = book.Title,
                BookImageUrl = book.ImageUrl
            };
        }

       
        public async Task<bool> RemoveFavouriteBookAsync(int favouriteBookId)
        {
            var favouriteBook = await _unitOfWork.FavoriteBookRepo.GetById(favouriteBookId);
            if (favouriteBook == null) return false;

            _unitOfWork.FavoriteBookRepo.Delete(favouriteBook.FavouriteBookId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Check if a book is already in user's favourites
        public async Task<bool> IsBookFavouritedAsync(string userId, int bookId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var existing = await _unitOfWork.FavoriteBookRepo
                .Query()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            return existing != null;
        }

        // Remove favourite book by userId and bookId
        public async Task<bool> RemoveFavouriteBookByUserAndBookAsync(string userId, int bookId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            var favouriteBook = await _unitOfWork.FavoriteBookRepo
                .Query()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favouriteBook == null) return false;

            _unitOfWork.FavoriteBookRepo.Delete(favouriteBook.FavouriteBookId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
