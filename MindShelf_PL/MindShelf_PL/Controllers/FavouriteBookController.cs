using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Dtos.FavouriteBookDto;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MindShelf.Controllers
{
    public class FavouriteBookController : Controller
    {
        private readonly IFavouriteBookService _favouriteBookService;

        public FavouriteBookController(IFavouriteBookService favouriteBookService)
        {
            _favouriteBookService = favouriteBookService;
        }

        // عرض كل المفضلة
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _favouriteBookService.GetAllFavouriteBooks(page, pageSize);
            if (!result.Success)
            {
                ViewBag.Error = result.Message;
                return View(new List<FavouriteBookResponseDto>());
            }

            return View(result.Data);
        }

        // إضافة كتاب للمفضلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int bookId)
        {
            if (bookId <= 0)
                return Json(new { success = false, message = "معرف الكتاب غير صالح" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "المستخدم غير مسجل دخول" });

            var favouriteBook = await _favouriteBookService.AddFavouriteBookAsync(userId, bookId);

            if (favouriteBook == null)
                return Json(new { success = false, message = "لم يتم العثور على الكتاب أو حدث خطأ" });

            return Json(new { success = true, message = "تمت إضافة الكتاب إلى المفضلة", data = favouriteBook });
        }

        // إزالة كتاب من المفضلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int favouriteBookId)
        {
            if (favouriteBookId <= 0)
                return Json(new { success = false, message = "معرف غير صالح" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "المستخدم غير مسجل دخول" });

            var result = await _favouriteBookService.RemoveFavouriteBookAsync(favouriteBookId);

            if (!result)
                return Json(new { success = false, message = "الكتاب غير موجود في المفضلة" });

            return Json(new { success = true, message = "تمت إزالة الكتاب من المفضلة" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int bookId)
        {
            if (bookId <= 0)
                return Json(new { success = false, message = "معرف الكتاب غير صالح" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "المستخدم غير مسجل دخول" });

            var existing = await _favouriteBookService.GetAllFavouriteBooks(1, int.MaxValue);
            if (!existing.Success || existing.Data == null)
                return Json(new { success = false, message = "حدث خطأ أثناء جلب المفضلة" });

            var favBook = existing.Data.FirstOrDefault(f => f.BookId == bookId && f.UserId == userId);

            if (favBook != null)
            {
                var removed = await _favouriteBookService.RemoveFavouriteBookAsync(favBook.FavouriteBookId);
                return Json(new
                {
                    success = removed,
                    message = removed ? "تمت إزالة الكتاب من المفضلة" : "خطأ أثناء الحذف",
                    removed = true
                });
            }
            else
            {
                var added = await _favouriteBookService.AddFavouriteBookAsync(userId, bookId);
                return Json(new
                {
                    success = added != null,
                    message = added != null ? "تمت إضافة الكتاب إلى المفضلة" : "خطأ أثناء الإضافة",
                    removed = false,
                    data = added
                });
            }
        }


    }
}
