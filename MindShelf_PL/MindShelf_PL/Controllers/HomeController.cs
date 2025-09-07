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
        private readonly IAuthorServies _authorServices;
        private readonly IEventServices _eventServices;

        public HomeController(ILogger<HomeController> logger, IBookServies bookServices, IAuthorServies authorServices, IEventServices eventServices)
        {
            _logger = logger;
            _bookServices = bookServices;
            _authorServices = authorServices;
            _eventServices = eventServices;
            _eventServices = eventServices;
        }

        public async Task<IActionResult> Index()
        {
            var topRatedBooksResponse = await _bookServices.GetTopRatedBooksAsync(4);
            ViewBag.TopRatedBooks = topRatedBooksResponse.Data ?? new List<MindShelf_BL.Dtos.BookDto.BookResponseDto>();
            
            var popularAuthorsResponse = await _authorServices.GetPopularAuthorsAsync(3);
            ViewBag.PopularAuthors = popularAuthorsResponse.Data ?? new List<MindShelf_BL.Dtos.AuthorDto.AuthorResponseDto>();

            var upcomingEventsResponse = await _eventServices.GetAllEvents(); // „À·« ‰ÃÌ» 4 ›⁄«·Ì« 
            ViewBag.UpcomingEvents = upcomingEventsResponse.Data ?? new List<MindShelf_BL.Dtos.EventDtos.EventResponseDto>();

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
