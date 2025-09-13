using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using MindShelf_BL.Dtos.AuthorDto;
using MindShelf_BL.Dtos.BookDto;
using MindShelf_BL.Dtos.CategoryDto;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_DAL.Models;
using MindShelf_PL.Hubs;
using MindShelf_PL.Models;
using MindShelf_PL.Hubs;
using System.Diagnostics;

namespace MindShelf_MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookServies _bookService;
        private readonly IAuthorServies _authorService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<BookNotificationHub> _hubContext;

        public BooksController(
            IBookServies bookService
            , IWebHostEnvironment env
            , IAuthorServies authorService
            , ICategoryService categoryService
            , IHubContext<BookNotificationHub> hubContext
            )
        {
            _bookService = bookService;
            _env = env;
            _authorService = authorService;
            _categoryService = categoryService;
            _hubContext = hubContext;
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return "/Images/books/default-book.jpg"; // Default image path

            var uploadPath = Path.Combine(_env.WebRootPath, "Images/books");
            Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/Images/books/" + fileName;
        }

        private IActionResult ErrorResult(string message)
        {
            return View("SearchError", new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int? authorId, int page = 1, int pageSize = 12)
        {

            var response = await _bookService.GetAllBook(page, pageSize);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            var books = response.Data.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                books = books.Where(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }


            var categoriesResponse = await _categoryService.GetAllCategories();
            ViewBag.Categories = categoriesResponse.StatusCode == 200 && categoriesResponse.Data != null
                ? categoriesResponse.Data
                : new List<CategoryResponseDto>();

            var authorsResponse = await _authorService.GetAllAuthor(1, 100);
            ViewBag.Authors = authorsResponse.StatusCode == 200 && authorsResponse.Data != null
                ? authorsResponse.Data
                : new List<AuthorResponseDto>();


            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedAuthorId = authorId;

            int totalBooks = response.TotalPages ?? books.Count();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalBooks = totalBooks; 

            return View(books.ToList());
        }


        public async Task<IActionResult> Details(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // جلب المؤلفين
            var authorsResponse = await _authorService.GetAllAuthor(1, 100);
            var authorsList = authorsResponse.Data ?? new List<AuthorResponseDto>();

            // تحويل المؤلفين إلى SelectListItem وإضافة عنصر افتراضي
            var authorsListItems = new List<SelectListItem>
    {
        new SelectListItem { Text = "-- اختر المؤلف --", Value = "" }
    };
            authorsListItems.AddRange(authorsList.Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.AuthorId.ToString()
            }));
            ViewBag.Authors = authorsListItems;

            // جلب الفئات
            var categoriesResponse = await _categoryService.GetAllCategories();
            var categoriesList = categoriesResponse.Data ?? new List<CategoryResponseDto>();

            // تحويل الفئات إلى SelectListItem وإضافة عنصر افتراضي
            var categoriesListItems = new List<SelectListItem>
    {
        new SelectListItem { Text = "-- اختر الفئة --", Value = "" }
    };
            categoriesListItems.AddRange(categoriesList.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            }));
            ViewBag.Categories = categoriesListItems;

            // تهيئة DTO مع تاريخ النشر الافتراضي
            var dto = new CreateBookDto
            {
                PublishedDate = DateTime.Today
            };

            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateBookDto dto, IFormFile? imageFile)
        {
            // إعادة تحميل الـ ViewBag في حال وجود خطأ في الـ ModelState
            var authorsResponse = await _authorService.GetAllAuthor(1, 10);
            var authorsList = authorsResponse.Data ?? new List<AuthorResponseDto>();
            var authorsListItems = new List<SelectListItem>
    {
        new SelectListItem { Text = "-- اختر المؤلف --", Value = "" }
    };
            authorsListItems.AddRange(authorsList.Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.AuthorId.ToString()
            }));
            ViewBag.Authors = authorsListItems;

            var categoriesResponse = await _categoryService.GetAllCategories();
            var categoriesList = categoriesResponse.Data ?? new List<CategoryResponseDto>();
            var categoriesListItems = new List<SelectListItem>
    {
        new SelectListItem { Text = "-- اختر الفئة --", Value = "" }
    };
            categoriesListItems.AddRange(categoriesList.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            }));
            ViewBag.Categories = categoriesListItems;

            // التحقق من صلاحية البيانات
            if (!ModelState.IsValid) return View(dto);

            // التحقق من وجود المؤلف
            if (!authorsList.Any(a => a.AuthorId == dto.AuthorId))
            {
                ModelState.AddModelError("", "المؤلف المحدد غير موجود");
                return View(dto);
            }

            // التحقق من وجود الفئة
            if (!categoriesList.Any(c => c.CategoryId == dto.CategoryId))
            {
                ModelState.AddModelError("", "الفئة المحددة غير موجودة");
                return View(dto);
            }

            // حفظ الصورة
            dto.ImageUrl = await SaveImageAsync(dto.ImageFile ?? imageFile);

            // إنشاء الكتاب
            var response = await _bookService.CreateBookAsync(dto);
            if (response.StatusCode != 201 || response.Data == null)
            {
                ModelState.AddModelError("", response.Message);
                return View(dto);
            }
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"كتاب جديد: {dto.Title}");

            // إرسال إشعار لجميع المستخدمين عن الكتاب الجديد
            try
            {
                await _hubContext.Clients.Group("BookNotifications").SendAsync("NewBookAdded", new
                {
                    BookId = response.Data.BookId,
                    Title = response.Data.Title,
                    AuthorName = response.Data.AuthorName,
                    CategoryName = response.Data.CategoryName,
                    Price = response.Data.Price,
                    ImageUrl = response.Data.ImageUrl,
                    AddedAt = DateTime.UtcNow
                });
                System.Diagnostics.Debug.WriteLine($"Book notification sent for: {response.Data.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending book notification: {ex.Message}");
                // Don't fail the book creation if notification fails
            }

            TempData["Success"] = "تمت إضافة الكتاب بنجاح ✅";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);
            var dto = new UpdateBookDto
            {
                BookId = id,
                PublishedDate = response.Data.PublishedDate,
                ImageUrl = response.Data.ImageUrl,
                Price = response.Data.Price,
                State = response.Data.State,
                Title = response.Data.Title,
                Description = response.Data.Description,
                Rating = response.Data.Rating,
                AuthorId = response.Data.AuthorId,
                CategoryId = response.Data.CategoryId
            };

            // Load dropdowns
            var authorsResponse = await _authorService.GetAllAuthor(1, 100);
            ViewBag.Authors = authorsResponse.Data?.Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.AuthorId.ToString()
            }).ToList() ?? new List<SelectListItem>();

            var categoriesResponse = await _categoryService.GetAllCategories();
            ViewBag.Categories = categoriesResponse.Data?.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.CategoryId.ToString()
            }).ToList() ?? new List<SelectListItem>();

            return View(dto);
        }


        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _bookService.GetBookByIdAsync(id);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return View(response.Data);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int BookId)
        {
            var response = await _bookService.DeleteBookAsync(BookId);
            if (response.StatusCode != 200 || !response.Data)
                return ErrorResult(response.Message);

            TempData["Success"] = "تم حذف الكتاب ❌";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var response = await _bookService.SearchBooksAsync(searchTerm);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            return PartialView("_Search", response.Data);
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

        public async Task<IActionResult> Filter(int? categoryId, int? authorId, string searchTerm)
        {
            var response = await _bookService.FilterBooksAsync(categoryId, authorId, searchTerm);
            if (response.StatusCode != 200 || response.Data == null)
                return ErrorResult(response.Message);

            // repopulate dropdowns
            var categories = await _categoryService.GetAllCategories();
            var authors = await _authorService.GetAllAuthor(1,10);

            ViewBag.Categories = categories.Data;
            ViewBag.Authors = authors.Data;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedAuthorId = authorId;
            ViewBag.SearchTerm = searchTerm;


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