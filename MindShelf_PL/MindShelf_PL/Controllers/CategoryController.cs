using Azure;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.CategoryDto;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_DAL.Models;

namespace MindShelf_PL.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryservice;
        public CategoryController(ICategoryService _categoryservice)
        {
            this._categoryservice = _categoryservice;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryservice.GetAllCategories();
            if (!categories.Success)
            {
                ViewBag.Error = categories.Message;
                return View("Error");
            }
            return View(categories.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var Category = await _categoryservice.GetCategoryDetails(id);
            if (!Category.Success)
            {
                ViewBag.Error = Category.Message;
                return View("Error");
            }
            return View(Category.Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var Category = await _categoryservice.CreateCategory(model);
            if (!Category.Success)
            {
                ModelState.AddModelError("", Category.Message);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var Category = await _categoryservice.GetCategoryById(Id);
            if (!Category.Success)
            {
                return NotFound();
            }

            var model = new UpateCategoryDto
            {
                Name = Category.Data.Name,
                Description = Category.Data.Books != null ? string.Join(", ", Category.Data.Books.Select(b => b.Title)) : "" //
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpateCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var Category = await _categoryservice.UpdateCategoryAsync(id, model);
            if (!Category.Success)
            {
                ModelState.AddModelError("", Category.Message);
                return View(model);
            }

            return RedirectToAction(("Index"));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var Category = await _categoryservice.GetCategoryById(id);
            if (!Category.Success)
            {
                return NotFound();
            }
            return View(Category);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Category = await _categoryservice.DeleteCategoryAsync(id);
            if (!Category.Success)
            {
                return NotFound();
            }
            return RedirectToAction("Index");
        }
    }
}

