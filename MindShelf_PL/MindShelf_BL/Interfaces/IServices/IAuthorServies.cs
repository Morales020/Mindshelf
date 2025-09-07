using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IAuthorServies
    {
        Task<ResponseMVC<IEnumerable<AuthorResponseDto>>> GetAllAuthor(int page, int pageSize);
        Task<ResponseMVC<AuthorResponseDto>> GetAuthorById(int id);
        Task<ResponseMVC<string>> CreateAuthor(AuthorCreateDto authorDto);
        Task<ResponseMVC<string>> UpdateAuthor(int id, AuthorUpdateDto authorDto);
        Task<ResponseMVC<string>> DeleteAuthor(int id);
        Task<ResponseMVC<IEnumerable<AuthorResponseDto>>> GetPopularAuthorsAsync(int count = 3);

    }
}
