using Azure;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IBookServies
    {
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetAllBook (int page, int pageSize);
    }
}
