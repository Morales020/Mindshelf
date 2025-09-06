using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.CategoryDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace MindShelf_BL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly UnitOfWork _unitOfWork;
        public CategoryService(UnitOfWork unitwork)
        {
            _unitOfWork = unitwork;
        }

        public async Task<ResponseMVC<CreateCategoryDto>> CreateCategory(CreateCategoryDto createDto)
        {
            try
            {
                var normalizedName = createDto.Name?.Trim().ToLower();

                // Check duplicate
                var exist = await _unitOfWork.CategoryRepo.Query()
                    .AnyAsync(c => c.Name.ToLower() == normalizedName);

                if (exist)
                {
                    return ResponseMVC<CreateCategoryDto>.ErrorResponse("Category with this name already exists.", 400);
                }

                // Map DTO → Entity
                var category = new Category
                {
                    Name = createDto.Name.Trim(),
                    Description = createDto.Description
                };

                await _unitOfWork.CategoryRepo.Add(category);
                await _unitOfWork.SaveChangesAsync();

                var result = new CreateCategoryDto
                {
                    Name = category.Name,
                    Description = category.Description
                };

                return ResponseMVC<CreateCategoryDto>.SuccessResponse(result, "Category created successfully", 201);
            }
            catch (Exception ex)
            {
                return ResponseMVC<CreateCategoryDto>.ErrorResponse(ex.Message, 500);
            }
        }


        public async Task<ResponseMVC<IEnumerable<CategoryResponseDto>>> GetAllCategories()
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepo
                    .Query()
                    .Include(c => c.Books)
                    .ToListAsync();

                var result = categories.Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    Books = c.Books.ToList()
                }).ToList();

                return ResponseMVC<IEnumerable<CategoryResponseDto>>.SuccessResponse(result, "Categories retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<IEnumerable<CategoryResponseDto>>.ErrorResponse(ex.Message, 500);
            }
        }


        public async Task<ResponseMVC<CategoryResponseDto>> GetCategoryById(int id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepo
                    .Query()
                    .Include(c => c.Books)
                    .FirstOrDefaultAsync(x => x.CategoryId == id);

                if (category == null)
                {
                    return ResponseMVC<CategoryResponseDto>.ErrorResponse("Category not found", 404);
                }

                var result = new CategoryResponseDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    Books = category.Books.ToList()
                };

                return ResponseMVC<CategoryResponseDto>.SuccessResponse(result, "Category retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<CategoryResponseDto>.ErrorResponse(ex.Message, 500);
            }
        }


        public async Task<ResponseMVC<CategoryDetailsDto>> GetCategoryDetails(int id)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepo
                    .Query()
                    .Include(c => c.Books)
                    .ThenInclude(b => b.Author) 
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    return ResponseMVC<CategoryDetailsDto>.ErrorResponse("Category not found", 404);
                }

                var result = new CategoryDetailsDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    Books = category.Books.ToList()
                };

                return ResponseMVC<CategoryDetailsDto>.SuccessResponse(result, "Category details retrieved successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<CategoryDetailsDto>.ErrorResponse(ex.Message, 500);
            }
        }


        public async Task<ResponseMVC<bool>> UpdateCategoryAsync(int id, UpateCategoryDto categoryDto)
        {
            try
            {
                var category = await _unitOfWork.CategoryRepo.GetById(id);

                if (category == null)
                {
                    return ResponseMVC<bool>.ErrorResponse("Category not found", 404);
                }

                // Overwrite 
                category.Name = categoryDto.Name.Trim();
                category.Description = categoryDto.Description;

                _unitOfWork.CategoryRepo.Update(category);
                await _unitOfWork.SaveChangesAsync();

                return ResponseMVC<bool>.SuccessResponse(true, "Category updated successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<bool>.ErrorResponse(ex.Message, 500);
            }
        }

        public async Task<ResponseMVC<bool>> DeleteCategoryAsync(int id)
        {
            try
            {
                var deleted = await _unitOfWork.CategoryRepo.Delete(id);

                if (!deleted)
                {
                    return ResponseMVC<bool>.ErrorResponse("Category not found", 404);
                }

                await _unitOfWork.SaveChangesAsync();

                return ResponseMVC<bool>.SuccessResponse(true, "Category deleted successfully", 200);
            }
            catch (Exception ex)
            {
                return ResponseMVC<bool>.ErrorResponse(ex.Message, 500);
            }
        }

    }
}
