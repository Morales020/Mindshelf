using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.EventDtos;
using MindShelf_BL.Dtos.OrderDtos;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_DAL.Models;

namespace MindShelf_PL.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventServices _eventService;

        public EventController(IEventServices eventService)
        {
            _eventService = eventService;
        }

        // GET: /Event
        public async Task<IActionResult> Index()
        {
            var result = await _eventService.GetAllEvents();
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.Response = result;
            return View(result.Data);
        }

        // GET: /Event/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var result = await _eventService.GetEventDetails(id);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.Response = result;
            return View(result.Data);
        }

        // GET: /Event/Create
        [HttpGet]
        public IActionResult Create()
        {
            var response = ResponseMVC<CreateEventDto>.SuccessResponse(new CreateEventDto());
            ViewBag.Response = response;
            return View(response.Data);
        }

        // POST: /Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = ResponseMVC<CreateEventDto>.ErrorResponse("Invalid model state");
                ViewBag.Response = errorResponse;
                return View(eventDto);
            }

            var result = await _eventService.CreateEvent(eventDto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.Response = result;
                return View(eventDto);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _eventService.GetEventById(id);
            if (!result.Success)
                return View("Error", result.Message);

            var dto = new UpdateEventDto
            {
                Title = result.Data.Title,
                StartingDate = result.Data.StartingDate,
                EndingDate = result.Data.EndingDate,
                Location = result.Data.Location,
                IsOnline = result.Data.IsOnline,
                IsActive = result.Data.IsActive
            };

            var response = ResponseMVC<UpdateEventDto>.SuccessResponse(dto);
            ViewBag.Response = response;
            return View(dto);
        }

        // POST: /Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = ResponseMVC<UpdateEventDto>.ErrorResponse("Invalid model state");
                ViewBag.Response = errorResponse;
                return View(eventDto);
            }

            var result = await _eventService.UpdateEventAsync(id, eventDto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.Response = result;
                return View(eventDto);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _eventService.GetEventById(id);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.Response = result;
            return View(result.Data);
        }

        // POST: /Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _eventService.DeleteEventAsync(id);
            if (!result.Success)
            {
                ViewBag.Response = result;
                return BadRequest(result.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
