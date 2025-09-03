using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Interfaces.IServices;

namespace MindShelf_PL.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        [Route("Payment/CreateCheckoutSession")]
        public async Task<JsonResult> CreateCheckoutSession()
        {
            // ممكن تخليهم جايين من الفيو بدل ما هما ثابتين
            decimal amount = 250;
            int orderId = 123;

            var sessionId = await _paymentService.CreateCheckoutSessionAsync(amount, orderId);

            return Json(new { id = sessionId });
        }

        public IActionResult Success(string session_id)
        {
            ViewBag.SessionId = session_id;
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }
    }
}
