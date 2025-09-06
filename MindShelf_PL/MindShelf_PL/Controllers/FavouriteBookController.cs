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

        // Toggle favourite status (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavourite(int bookId)
        {
            if (bookId <= 0)
            {
                return Json(new { success = false, message = "معرف الكتاب غير صالح" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول", redirect = Url.Action("Login", "Account") });
            }

            try
            {
                // Check if book is already favourited
                var isFavourited = await _favouriteBookService.IsBookFavouritedAsync(userId, bookId);
                
                if (isFavourited)
                {
                    // Remove from favourites
                    var removed = await _favouriteBookService.RemoveFavouriteBookByUserAndBookAsync(userId, bookId);
                    if (removed)
                    {
                        return Json(new { success = true, isFavourited = false, message = "تمت إزالة الكتاب من المفضلة" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "حدث خطأ أثناء إزالة الكتاب من المفضلة" });
                    }
                }
                else
                {
                    // Add to favourites
                    var favouriteBook = await _favouriteBookService.AddFavouriteBookAsync(userId, bookId);
                    if (favouriteBook != null)
                    {
                        return Json(new { success = true, isFavourited = true, message = "تمت إضافة الكتاب إلى المفضلة" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "حدث خطأ أثناء إضافة الكتاب إلى المفضلة" });
                    }
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ غير متوقع" });
            }
        }

        // Check favourite status (AJAX)
        [HttpGet]
        public async Task<IActionResult> CheckFavouriteStatus(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { isFavourited = false });
            }

            var isFavourited = await _favouriteBookService.IsBookFavouritedAsync(userId, bookId);
            return Json(new { isFavourited = isFavourited });
        }

        // Remove from favourites (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavouriteAjax(int bookId)
        {
            if (bookId <= 0)
            {
                return Json(new { success = false, message = "معرف الكتاب غير صالح" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول" });
            }

            try
            {
                var removed = await _favouriteBookService.RemoveFavouriteBookByUserAndBookAsync(userId, bookId);
                if (!removed)
                {
                    return Json(new { success = true, message = "تمت إزالة الكتاب من المفضلة" });
                }
                else
                {
                    return Json(new { success = false, message = "الكتاب غير موجود في المفضلة" });
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ غير متوقع" });
            }
        }
    }
}