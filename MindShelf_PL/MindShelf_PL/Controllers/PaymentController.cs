using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using MindShelf_BL.Dtos.CartsDto;

namespace MindShelf_PL.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICartServices _cartServices;

        public PaymentController(IPaymentService paymentService, ICartServices cartServices)
        {
            _paymentService = paymentService;
            _cartServices = cartServices;
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        [Route("Payment/CreateCheckoutSession")]
        public async Task<JsonResult> CreateCheckoutSession([FromBody] CheckoutRequestDto request)
        {
            try
            {
                // Calculate total from cart items
                decimal amount = 0;
                if (request.OrderItems != null && request.OrderItems.Any())
                {
                    // Fix: Use TotalPrice instead of BookId * Quantity
                    amount = request.OrderItems.Sum(x => x.TotalPrice);
                }
                else
                {
                    // Fallback to default amount if no cart items
                    amount = 200;
                }

                // Generate order ID (you might want to save this to database)
                int orderId = new Random().Next(1000, 9999);

                // Pass the address and order items from the checkout request
                var sessionId = await _paymentService.CreateCheckoutSessionAsync(amount, orderId, request.Address, request.OrderItems);

                return Json(new { id = sessionId });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> Success(string session_id)
        {
            try
            {
                // Clear the cart after successful payment
                var userName = User.Identity?.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    await _cartServices.ClearCart(userName);
                }

                ViewBag.SessionId = session_id;
                ViewBag.SuccessMessage = "تم الدفع بنجاح وتم مسح السلة";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.SessionId = session_id;
                ViewBag.ErrorMessage = "تم الدفع بنجاح ولكن حدث خطأ في مسح السلة";
                return View();
            }
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }
}