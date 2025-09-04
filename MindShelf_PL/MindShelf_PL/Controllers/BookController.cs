using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_PL.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using MindShelf_BL.Services;

namespace MindShelf_MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookServies _bookService;

        private readonly IWebHostEnvironment _env;

        public BooksController(IBookServies bookService, IWebHostEnvironment env)
        {
            _bookService = bookService;
            _env = env;
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadPath = Path.Combine(_env.WebRootPath, "images/books");
            Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/images/books/" + fileName;
        }

        private IActionResult ErrorResult(string message)
        {
            return View("Error", new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var response = await _bookService.GetAllBook(page, pageSize);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categoryService.GetAll(), "CategoryId", "Name");
            ViewBag.Authors = new SelectList(_authorService.GetAll(), "AuthorId", "Name");

            var dto = new CreateBookDto
            {
                PublishedDate = DateTime.Today
            };

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(dto);

            dto.ImageUrl = await SaveImageAsync(imageFile);

            var response = await _bookService.CreateBookAsync(dto);
            if (response.StatusCode != 201 || response.Data == null)
            {
                ModelState.AddModelError("", response.Message);
                return View(dto);
            }

            TempData["Success"] = "تمت إضافة الكتاب بنجاح ✅";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            var book = response.Data;
            return View(new UpdateBookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price,
                ImageUrl = book.ImageUrl,
                State = book.State,
                PublishedDate = book.PublishedDate
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateBookDto dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return View(dto);

            var newImage = await SaveImageAsync(imageFile);
            if (!string.IsNullOrEmpty(newImage))
                dto.ImageUrl = newImage;

            var response = await _bookService.UpdateBookAsync(dto);
            if (response.StatusCode != 200 || response.Data == null)
            {
                ModelState.AddModelError("", response.Message);
                return View(dto);
            }

            TempData["Success"] = "تم تحديث الكتاب بنجاح ✅";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _bookService.DeleteBookAsync(id);
            if (response.StatusCode != 200 || !response.Data)
                return ErrorResult(response.Message);

            TempData["Success"] = "تم حذف الكتاب ❌";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string term)
        {
            var response = await _bookService.SearchBooksAsync(term);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View("Index", response.Data);
        }

        public async Task<IActionResult> ByCategory(int categoryId)
        {
            var response = await _bookService.GetBooksByCategoryAsync(categoryId);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View("Index", response.Data);
        }

        public async Task<IActionResult> ByAuthor(int authorId)
        {
            var response = await _bookService.GetBooksByAuthorAsync(authorId);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View("Index", response.Data);
        }

        public async Task<IActionResult> NewReleases()
        {
            var response = await _bookService.GetNewReleasesAsync();
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View("Index", response.Data);
        }
    }
}
