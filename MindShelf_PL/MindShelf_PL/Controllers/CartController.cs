using Microsoft.AspNetCore.Mvc;
using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using System.Threading.Tasks;

namespace MindShelf_PL.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartServices _cartServices;
        
        public CartController(ICartServices cartServices)
        {
            _cartServices = cartServices;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCartByUserName(string userName)
        {
            var cart = await _cartServices.GetCartByUserName(userName);
            return View("Index", cart.Data);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", addToCartDto);
            }
            
            var cart = await _cartServices.AddToCart(addToCartDto);
            if (!cart.Success)
            {
                ModelState.AddModelError("", cart.Message);
                return View("Index", addToCartDto);
            }
            
            return View("Index", cart.Data);
        }
        
        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(RemoveCartItemDto removeCartItemDto)
        {
            var cart = await _cartServices.RemoveCartItem(removeCartItemDto);
            return View("Index", cart.Data);
        }
        
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(UpdateCartItemDto updateCartItemDto)
        {
            var cart = await _cartServices.UpdateCartItem(updateCartItemDto);
            return View("Index", cart.Data);
        }
        
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutRequestDto checkoutRequestDto)
        {
            var cart = await _cartServices.CheckoutAsync(checkoutRequestDto);
            return View("Index", cart.Data);
        }
    }
}