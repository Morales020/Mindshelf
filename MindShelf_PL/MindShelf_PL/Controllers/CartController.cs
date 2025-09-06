using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Added for List

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

        #region AJAX Methods
         [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                var updateDto = new UpdateCartItemDto
                {
                    CartItemId = cartItemId,
                    Quantity = quantity,
                    UserName = userName
                };

                var result = await _cartServices.UpdateCartItem(updateDto);
                
                if (result.Success)
                {
                    // Get updated cart data
                    var cart = await _cartServices.GetCartByUserName(userName);
                    var updatedItem = cart.Data?.CartItems?.FirstOrDefault(x => x.CartItemId == cartItemId);
                    
                    return Json(new { 
                        success = true, 
                        itemTotal = updatedItem?.TotalPrice ?? 0,
                        cartTotal = cart.Data?.CartItems?.Sum(x => x.TotalPrice) ?? 0,
                        itemCount = cart.Data?.CartItems?.Sum(x => x.Quantity) ?? 0  // Fix: Sum of quantities, not count of items
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء التحديث" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveItem(int cartItemId)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                var result = await _cartServices.RemoveCartItem(new RemoveCartItemDto {
                    CartItemId = cartItemId,
                    UserName = userName
                });
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "تم حذف العنصر بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء حذف العنصر" });
            }
        }
        #endregion

        #region CRUD Operations
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
            return RedirectToAction("AddToCartSuccess"); // Instead of "Index", "Books"
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
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["Error"] = "يجب تسجيل الدخول أولاً";
                    return RedirectToAction("Login", "Account");
                }

                var cart = await _cartServices.GetCartByUserName(userName);
                if (cart.Data?.CartItems != null && cart.Data.CartItems.Any())
                {
                    // Remove all cart items using CartItemId
                    foreach (var item in cart.Data.CartItems)
                    {
                        var removeDto = new RemoveCartItemDto
                        {
                            CartItemId = item.CartItemId, // Use CartItemId
                            UserName = userName
                        };
                        await _cartServices.RemoveCartItem(removeDto);
                    }
                    
                    TempData["Success"] = "تم مسح السلة بالكامل";
                }
                else
                {
                    TempData["Info"] = "السلة فارغة بالفعل";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء مسح السلة";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdate([FromBody] List<UpdateCartItemDto> items)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                foreach (var item in items)
                {
                    item.UserName = userName;
                    await _cartServices.UpdateCartItem(item);
                }

                return Json(new { success = true, message = "تم تحديث السلة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء التحديث" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAdd([FromBody] List<AddToCartDto> items)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return Json(new { success = false, message = "المستخدم غير موجود" });
                }

                foreach (var item in items)
                {
                    item.UserName = userName;
                    item.UserId = user.Id;
                    await _cartServices.AddToCart(item);
                }

                return Json(new { success = true, message = "تم إضافة العناصر إلى السلة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الإضافة" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateItem(int bookId)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["Error"] = "يجب تسجيل الدخول أولاً";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    TempData["Error"] = "المستخدم غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                // Get current cart to find the item
                var cart = await _cartServices.GetCartByUserName(userName);
                var existingItem = cart.Data?.CartItems?.FirstOrDefault(x => x.BookId == bookId);
                
                if (existingItem != null)
                {
                    var addToCartDto = new AddToCartDto
                    {
                        BookId = bookId,
                        Quantity = existingItem.Quantity, // Duplicate with same quantity
                        UserName = userName,
                        UserId = user.Id
                    };

                    var result = await _cartServices.AddToCart(addToCartDto);
                    if (result.Success)
                    {
                        TempData["Success"] = "تم تكرار العنصر في السلة";
                    }
                    else
                    {
                        TempData["Error"] = result.Message;
                    }
                }
                else
                {
                    TempData["Error"] = "العنصر غير موجود في السلة";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تكرار العنصر";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToWishlist(int bookId)
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    TempData["Error"] = "يجب تسجيل الدخول أولاً";
                    return RedirectToAction(nameof(Index));
                }

                // Remove from cart
                var removeDto = new RemoveCartItemDto
                {
                    CartItemId = bookId,
                    UserName = userName
                };
                await _cartServices.RemoveCartItem(removeDto);

                // TODO: Add to wishlist service when available
                // await _wishlistServices.AddToWishlist(wishlistDto);

                TempData["Success"] = "تم نقل العنصر إلى قائمة الأمنيات";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء نقل العنصر";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { count = 0 });
                }

                var cart = await _cartServices.GetCartByUserName(userName);
                var count = cart.Data?.CartItems?.Count ?? 0;
                
                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartTotal()
        {
            try
            {
                var userName = User.Identity?.Name;
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { total = 0 });
                }

                var cart = await _cartServices.GetCartByUserName(userName);
                var total = cart.Data?.CartItems?.Sum(x => x.TotalPrice) ?? 0;
                
                return Json(new { total = total });
            }
            catch (Exception ex)
            {
                return Json(new { total = 0 });
            }
        }
      
        #endregion

        #region Checkout Operations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutRequestDto dto)
        {
            dto.UserName = User.Identity.Name;
            await _cartServices.CheckoutAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await _cartServices.GetCartByUserName(User.Identity.Name);
            if (cart.Data == null || !cart.Data.CartItems.Any())
            {
                TempData["Error"] = "السلة فارغة، لا يمكن إتمام الطلب";
                return RedirectToAction(nameof(Index));
            }
            
            return View(cart.Data);
        }
        #endregion

        #region Additional Views
        [HttpGet]
        public IActionResult AddToCartSuccess()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var cart = await _cartServices.GetCartByUserName(User.Identity.Name);
            return View(cart.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int cartItemId)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartServices.GetCartByUserName(userName);
            var item = cart.Data?.CartItems?.FirstOrDefault(x => x.CartItemId == cartItemId);
            
            if (item == null)
            {
                TempData["Error"] = "العنصر غير موجود";
                return RedirectToAction("Index");
            }

            return View(item);
        }
        #endregion
    }
}
