using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.OrderDtos;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;

namespace MindShelf_PL.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderServices _orderServices;

        public OrderController(IOrderServices orderServices)
        {
            _orderServices = orderServices;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _orderServices.GetAllOrdersAsync(pageNumber, pageSize);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = pageNumber;
            return View(result.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return NotFound();

            var totalAmountResult = await _orderServices.CalculateTotalAmountAsync(id);
            ViewBag.TotalAmount = totalAmountResult.Success ? totalAmountResult.Data : 0;
            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateOrderDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
                return View(orderDto);

            var result = await _orderServices.CreateOrderAsync(orderDto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(orderDto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditStatus(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, OrderState status)
        {
            var result = await _orderServices.UpdateOrderStatusAsync(id, status);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                var orderResult = await _orderServices.GetOrderByIdAsync(id);
                return View(orderResult.Data);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderServices.GetOrderByIdAsync(id);
            if (!result.Success)
                return NotFound();

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _orderServices.DeleteOrderAsync(id);
            if (!result.Success)
                return BadRequest(result.Message);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ByStatus(OrderState status, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _orderServices.GetOrdersByStatusAsync(status, pageNumber, pageSize);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.Status = status;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = pageNumber;
            return View("Index", result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ByUser(string userName)
        {
            var result = await _orderServices.GetOrdersByUserNameAsync(userName);
            if (!result.Success)
                return View("Error", result.Message);

            ViewBag.UserName = userName;
            return View("Index", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderServices.CancelOrder(id);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                var orderResult = await _orderServices.GetOrderByIdAsync(id);
                return View("Details", orderResult.Data);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
