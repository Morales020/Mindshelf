using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Dtos.FavouriteBookDto;
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
        public async Task<IActionResult> Add(string userId, int bookId)
        {
            var favouriteBook = await _favouriteBookService.AddFavouriteBookAsync(userId, bookId);
            if (favouriteBook == null)
            {
                TempData["Error"] = "User or Book not found.";
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Book added to favourites successfully.";
            return RedirectToAction("Index");
        }

        // إزالة كتاب من المفضلة
        [HttpPost]
        public async Task<IActionResult> Remove(int favouriteBookId)
        {
            var result = await _favouriteBookService.RemoveFavouriteBookAsync(favouriteBookId);
            if (!result)
            {
                TempData["Error"] = "Favourite book not found.";
            }
            else
            {
                TempData["Success"] = "Favourite book removed successfully.";
            }

            return RedirectToAction("Index");
        }
    }
}
