using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MindShelf_BL.Dtos.EventDtos;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using MindShelf_BL.UnitWork;

namespace MindShelf_PL.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventServices _eventService;
        private readonly UserManager<User> _userManager;

        public EventController(IEventServices eventService, UserManager<User> userManager)
        {
            _eventService = eventService;
            _userManager = userManager;
        }

        // GET: /Event - متاح للجميع
        public async Task<IActionResult> Index()
        {
            var result = await _eventService.GetAllEvents();
            if (!result.Success)
                return View("Error", result.Message);

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            ViewBag.IsAdmin = isAdmin;

            return View(result.Data);
        }

        // GET: /Event/Details/5 - متاح للجميع
        public async Task<IActionResult> Details(int id)
        {
            var result = await _eventService.GetEventDetails(id);
            if (!result.Success)
                return View("Error", result.Message);

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsLoggedIn = user != null;

            return View(result.Data);
        }

        // GET: /Event/Create - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpGet]
        
        public IActionResult Create()
        {
            var response = ResponseMVC<CreateEventDto>.SuccessResponse(new CreateEventDto
            {
                StartingDate = DateTime.Now.AddDays(1),
                EndingDate = DateTime.Now.AddDays(1).AddHours(2)
            });
            ViewBag.Response = response;
            return View(response.Data);
        }

        // POST: /Event/Create - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = ResponseMVC<CreateEventDto>.ErrorResponse("بيانات غير صحيحة");
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

            TempData["Success"] = "تم إنشاء الحدث بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Edit/5 - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _eventService.GetEventById(id);
            if (!result.Success)
                return View("Error", result.Message);

            var dto = new UpdateEventDto
            {
                Title = result.Data.Title,
                Description = result.Data.Description,
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

        // POST: /Event/Edit/5 - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = ResponseMVC<UpdateEventDto>.ErrorResponse("بيانات غير صحيحة");
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

            TempData["Success"] = "تم تحديث الحدث بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Delete/5 - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _eventService.GetEventById(id);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.Response = result;
            return View(result.Data);
        }

        // POST: /Event/Delete/5 - للأدمن فقط
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _eventService.DeleteEventAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "تم حذف الحدث بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> RegistrationDetails(int id)
        {
            var result = await _eventService.GetRegistrationById(id);
            if (!result.Success)
                return View("Error", result.Message);

            return View(result.Data);
        }

        // POST: /Event/Register/5 - تسجيل مستخدم في حدث
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForEvent(CreateEventRegistrationDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "يجب تسجيل الدخول أولاً";
                return RedirectToAction("Login", "Account");
            }
            dto.UserId = user.Id;
            dto.UserName = user.UserName;
            var result = await _eventService.RegisterForEvent( dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Details", new { id = dto.EventId});
            }

            TempData["Success"] = "تم تسجيلك في الحدث بنجاح";
            return RedirectToAction("Details", new { id = dto.EventId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRegistration(int id, int eventId)
        {
            var result = await _eventService.DeleteRegistration(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Details", new { id = eventId });
            }

            TempData["Success"] = "تم حذف التسجيل بنجاح";
            return RedirectToAction("Details", new { id = eventId });
        }


    }
}