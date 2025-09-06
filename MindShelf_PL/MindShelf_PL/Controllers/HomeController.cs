using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MindShelf_PL.Models;
using MindShelf_BL.Interfaces.IServices;

namespace MindShelf_PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookServies _bookServices;

        public HomeController(ILogger<HomeController> logger, IBookServies bookServices)
        {
            _logger = logger;
            _bookServices = bookServices;
        }

        public async Task<IActionResult> Index()
        {
            var topRatedBooksResponse = await _bookServices.GetTopRatedBooksAsync(4);
            ViewBag.TopRatedBooks = topRatedBooksResponse.Data ?? new List<MindShelf_BL.Dtos.BookDto.BookResponseDto>();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
