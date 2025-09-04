using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Dtos.FavouriteBookDto;
using MindShelf_BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IFavouriteBookService
    {
        Task<ResponseMVC<IEnumerable<FavouriteBookResponseDto>>> GetAllFavouriteBooks(int page, int pageSize);
        Task<FavouriteBookResponseDto> AddFavouriteBookAsync(string userId, int bookId);
        Task<bool> RemoveFavouriteBookAsync(int favouriteBookId);
    }
}
