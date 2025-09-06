using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Dtos.FavouriteBookDto;
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
    public class FavouriteBookService : IFavouriteBookService
    {
        private readonly UnitOfWork _UnitOfWork;

        public FavouriteBookService(UnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<ResponseMVC<IEnumerable<FavouriteBookResponseDto>>> GetAllFavouriteBooks(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var FavouriteBooks = await _UnitOfWork.FavoriteBookRepo
                .Query()
                .Include(s => s.Book)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            if (FavouriteBooks.Count == 0)
            {
                return new ResponseMVC<IEnumerable<FavouriteBookResponseDto>>(404, "Favourite book not found", null);
            }

            var FavouriteBookDto = FavouriteBooks.Select(FBook => new FavouriteBookResponseDto
            {
                FavouriteBookId = FBook.FavouriteBookId,
                BookId = FBook.BookId,
                UserId = FBook.UserId,
                UserName = FBook.UserName,
                AddedDate = FBook.AddedDate,
            }).ToList();


            return new ResponseMVC<IEnumerable<FavouriteBookResponseDto>>(200, "Success", FavouriteBookDto);
        }
        public async Task<FavouriteBookResponseDto> AddFavouriteBookAsync(string userId, int bookId)
        {
             var book = await _UnitOfWork._dbcontext.Books.FindAsync(bookId);
            if (book == null)
                return null;

            // تأكد ان نفس الكتاب مش متسجل للمستخدم ده قبل كده
            var existing = await _UnitOfWork.FavoriteBookRepo
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
                    AddedDate = existing.AddedDate
                };
            }

            
            var favouriteBook = new FavouriteBook
            {
                UserId = userId,
                BookId = bookId,
                AddedDate = DateTime.UtcNow,
                UserName = await _UnitOfWork._dbcontext.Users
                                .Where(u => u.Id == userId)
                                .Select(u => u.UserName)
                                .FirstOrDefaultAsync()
            };

            await _UnitOfWork.FavoriteBookRepo.Add(favouriteBook);
            await _UnitOfWork.SaveChangesAsync();

            return new FavouriteBookResponseDto
            {
                FavouriteBookId = favouriteBook.FavouriteBookId,
                BookId = favouriteBook.BookId,
                UserId = favouriteBook.UserId,
                UserName = favouriteBook.UserName,
                AddedDate = favouriteBook.AddedDate
            };
        }

        public async Task<bool> RemoveFavouriteBookAsync(int favouriteBookId)
        {
            var favouriteBook = await _UnitOfWork.FavoriteBookRepo.GetById(favouriteBookId);
            if (favouriteBook == null) return false;

            _UnitOfWork.FavoriteBookRepo.Delete(favouriteBook.FavouriteBookId);
            await _UnitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
