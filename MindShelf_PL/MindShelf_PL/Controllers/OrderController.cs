using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.OrderDtos;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using MindShelf_PL.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MindShelf_PL.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderServices _orderServices;

        public OrderController(IOrderServices orderServices)
        {
            _orderServices = orderServices;
        }

       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var result = await _orderServices.GetAllOrdersAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                userId: null, 
                status: status,
                fromDate: fromDate,
                toDate: toDate
            );

            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.StatusFilter = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(result.Data);
        }


        public async Task<IActionResult> Details(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return View("Error", result.Message);

            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateOrderDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _orderServices.CreateOrderAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(dto);
            }

            return RedirectToAction("Details", new { id = result.Data.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStatus(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return View("Error", result.Message);

            return View(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStatus(int id, OrderState status)
        {
            var result = await _orderServices.UpdateOrderStatusAsync(id, status);
            if (!result.Success)
                return View("Error", result.Message);

            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderServices.CancelOrder(id);

            if (!result.Success)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = result.Message // assuming your ErrorViewModel has a Message property
                };
                return View("Error", errorModel);
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }

            if (User.IsInRole("User"))
            {
                return RedirectToAction("UserOrders");
            }

            // fallback just in case
            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecalculateTotal(int id)
        {
            var result = await _orderServices.CalculateTotalAmountAsync(id);
            if (!result.Success)
                return View("Error", result.Message);

            TempData["Message"] = $"Total recalculated: {result.Data:C}";
            return RedirectToAction("Details", new { id });
        }

        
        public async Task<IActionResult> UserOrders(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
           
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _orderServices.GetAllOrdersAsync(userId, status, fromDate, toDate, pageNumber );

            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.StatusFilter = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.UserName = User.Identity?.Name;

            return View("UserOrders", result.Data); 
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return View("Error", result.Message);

            // Check if order can be deleted
            if (result.Data.OrderStatus == OrderState.Confirmed || 
                result.Data.OrderStatus == OrderState.Shipping || 
                result.Data.OrderStatus == OrderState.Delivered)
            {
                TempData["Error"] = "لا يمكن حذف الطلبات المؤكدة أو المشحونة أو المسلمة";
                return RedirectToAction("Index");
            }

            return View(result.Data); // show confirmation page
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Double-check order status before deletion
            var orderResult = await _orderServices.GetOrderByIdAsync(id);
            if (!orderResult.Success)
                return View("Error", orderResult.Message);

            if (orderResult.Data.OrderStatus == OrderState.Confirmed || 
                orderResult.Data.OrderStatus == OrderState.Shipping || 
                orderResult.Data.OrderStatus == OrderState.Delivered)
            {
                TempData["Error"] = "لا يمكن حذف الطلبات المؤكدة أو المشحونة أو المسلمة";
                return RedirectToAction("Index");
            }

            var result = await _orderServices.DeleteOrderAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            TempData["Message"] = "تم حذف الطلب بنجاح";
            return RedirectToAction("Index");
        }
    }
}
