using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using System;
using System.Threading.Tasks;

namespace MindShelf_PL.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartServices _cartServices;
        private readonly UserManager<User> _userManager;

        public CartController(ICartServices cartServices ,UserManager<User> userManager)
        {
            _cartServices = cartServices;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await _cartServices.GetCartByUserName(User.Identity.Name);
            return View(cart.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      
            
            public async Task<IActionResult> AddToCart(int BookId, int Quantity)
            {
           Console.WriteLine($"Received: BookId={BookId}, Quantity={Quantity}");

            if (BookId <= 0 || Quantity <= 0)
            {
                TempData["Error"] = "بيانات غير صالحة";
                return RedirectToAction("Index", "Books");
            }

            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "يجب تسجيل الدخول أولاً";
                return RedirectToAction("Login", "Account");
            }

            // جلب UserId من AspNetUsers بناءً على UserName
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction("Login", "Account");
            }

            var addToCartDto = new AddToCartDto
            {
                BookId = BookId,
                Quantity = Quantity,
                UserName = userName,
                UserId = user.Id // إضافة UserId للـ DTO
            };

            var cart = await _cartServices.AddToCart(addToCartDto);
            if (!cart.Success)
            {
                TempData["Error"] = cart.Message;
                return RedirectToAction("Index", "Books");
            }

            TempData["Success"] = "تم إضافة الكتاب إلى السلة بنجاح";
            return RedirectToAction("Index", "Books");
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCartItem(RemoveCartItemDto dto)
        {
            dto.UserName = User.Identity.Name;
            await _cartServices.RemoveCartItem(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto dto)
        {
            dto.UserName = User.Identity.Name;
            await _cartServices.UpdateCartItem(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutRequestDto dto)
        {
            dto.UserName = User.Identity.Name;
            await _cartServices.CheckoutAsync(dto);
            return RedirectToAction(nameof(Index));
        }
      
    }
}
