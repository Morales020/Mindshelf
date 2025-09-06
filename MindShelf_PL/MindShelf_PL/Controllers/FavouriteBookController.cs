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

        // صفحة عرض كل الكتب المفضلة
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "يجب تسجيل الدخول لعرض المفضلة";
                return RedirectToAction("Login", "Account");
            }

            var result = await _favouriteBookService.GetAllFavouriteBooks(userId, page, pageSize);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(new List<FavouriteBookResponseDto>());
            }

            return View(result.Data);
        }

        // إضافة كتاب للمفضلة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavourite(int bookId)
        {
            if (bookId <= 0)
            {
                TempData["Error"] = "معرف الكتاب غير صالح";
                return RedirectToAction("Index", "Books");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "يجب تسجيل الدخول لإضافة كتاب للمفضلة";
                return RedirectToAction("Login", "Account");
            }

            var favouriteBook = await _favouriteBookService.AddFavouriteBookAsync(userId, bookId);
            if (favouriteBook == null)
            {
                TempData["Error"] = "حدث خطأ أثناء إضافة الكتاب إلى المفضلة";
            }
            else
            {
                TempData["Success"] = "تمت إضافة الكتاب إلى المفضلة";
            }

            return RedirectToAction("Index", "Books");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavourite(int favouriteBookId)
        {
            if (favouriteBookId <= 0)
            {
                TempData["Error"] = "معرف غير صالح";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "يجب تسجيل الدخول لإزالة كتاب من المفضلة";
                return RedirectToAction("Login", "Account");
            }

            var result = await _favouriteBookService.RemoveFavouriteBookAsync(favouriteBookId);
            if (!result)
            {
                TempData["Error"] = "الكتاب غير موجود في المفضلة";
            }
            else
            {
                TempData["Success"] = "تمت إزالة الكتاب من المفضلة";
            }

            return RedirectToAction("Index");
        }
    }
}