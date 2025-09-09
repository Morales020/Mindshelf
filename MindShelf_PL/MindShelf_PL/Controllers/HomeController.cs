using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MindShelf_PL.Models;
using Microsoft.AspNetCore.Identity;
using MindShelf_DAL.Models;
using MindShelf_BL.Interfaces.IServices;

namespace MindShelf_PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBookServies _bookServices;
        private readonly IAuthorServies _authorServices;
        private readonly IEventServices _eventServices;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, IBookServies bookServices, IAuthorServies authorServices, IEventServices eventServices, ICategoryService categoryService, UserManager<User> userManager)
        {
            _logger = logger;
            _bookServices = bookServices;
            _authorServices = authorServices;
            _eventServices = eventServices;
            _eventServices = eventServices;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var topRatedBooksResponse = await _bookServices.GetTopRatedBooksAsync(4);
            ViewBag.TopRatedBooks = topRatedBooksResponse.Data ?? new List<MindShelf_BL.Dtos.BookDto.BookResponseDto>();
            
            var popularAuthorsResponse = await _authorServices.GetPopularAuthorsAsync(3);
            ViewBag.PopularAuthors = popularAuthorsResponse.Data ?? new List<MindShelf_BL.Dtos.AuthorDto.AuthorResponseDto>();

            var upcomingEventsResponse = await _eventServices.GetAllEvents(); // ����� ���� 4 �������
            ViewBag.UpcomingEvents = upcomingEventsResponse.Data ?? new List<MindShelf_BL.Dtos.EventDtos.EventResponseDto>();

            var categoriesResponse = await _categoryService.GetAllCategories();
            ViewBag.Categories = categoriesResponse.Data ?? new List<MindShelf_BL.Dtos.CategoryDto.CategoryResponseDto>();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.IsLoggedIn = currentUser != null;
            ViewBag.CurrentUserId = currentUser?.Id;

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
