using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IBookServies
    {
        // Get
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetAllBook(int page, int pageSize);
        Task<ResponseMVC<BookResponseDto>> GetBookByIdAsync(int id);
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetBooksByAuthorAsync(int authorId);
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetBooksByCategoryAsync(int categoryId);
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetNewReleasesAsync();
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> GetTopRatedBooksAsync(int count = 4);
        Task<ResponseMVC<IEnumerable<BookResponseDto>>> SearchBooksAsync(string searchTerm);

        // Create, Update, Delete
        Task<ResponseMVC<BookResponseDto>> CreateBookAsync(CreateBookDto createBookDto);
        Task<ResponseMVC<BookResponseDto>> UpdateBookAsync(UpdateBookDto updateBookDto);
        Task<ResponseMVC<bool>> UpdateBookRatingAsync(int bookId, double averageRating, int reviewCount);
        Task<ResponseMVC<bool>> DeleteBookAsync(int id);
    }
}
