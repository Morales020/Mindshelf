using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Dtos.CategoryDto;
using MindShelf_BL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface ICategoryService
    {
        Task<ResponseMVC<IEnumerable<CategoryResponseDto>>> GetAllCategories();
        Task<ResponseMVC<CategoryResponseDto>> GetCategoryById(int Id);
        Task<ResponseMVC<CategoryDetailsDto>> GetCategoryDetails(int Id);

        Task<ResponseMVC<CreateCategoryDto>> CreateCategory(CreateCategoryDto createDto);
        // Update
        Task<ResponseMVC<bool>> UpdateCategoryAsync(int id, UpateCategoryDto categoryDto);

        // delete
        Task<ResponseMVC<bool>> DeleteCategoryAsync(int id);
    }
}
