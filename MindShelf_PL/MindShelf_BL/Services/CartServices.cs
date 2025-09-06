using MindShelf_BL.UnitWork;
using MindShelf_BL.Helper;
using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;
using MindShelf_DAL.Models;

namespace MindShelf_BL.Services
{
    public class CartServices : ICartServices
    {
        private readonly UnitOfWork _unitOfWork;

        public CartServices(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region GetCartByUserName
        public async Task<ResponseMVC<CartResponseDto>> GetCartByUserName(string userName)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.Query()
                .Include(c => c.ShoppingCartItems)
                    .ThenInclude(ci => ci.Book)
                        .ThenInclude(b => b.Author)
                .Include(c => c.ShoppingCartItems)
                    .ThenInclude(ci => ci.Book.Category)
                .FirstOrDefaultAsync(c => c.UserName == userName);

            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }

            var cartResponse = new CartResponseDto
            {
                ShoppingCartId = cart.ShoppingCartId,
                UserName = cart.UserName,
                CreatedAt = cart.CreatedAt,
                IsCheckedOut = cart.IsCheckedOut,
                CartItems = cart.ShoppingCartItems.Select(ci => new CartItemResponseDto
                {
                    CartItemId = ci.CartItemId,
                    BookName = ci.Book.Title,
                    BookImageUrl = ci.Book.ImageUrl,
                    BookDescription = ci.Book.Description,
                    BookAuthor = ci.Book.Author?.Name ?? "Unknown",
                    BookCategory = ci.Book.Category?.Name ?? "Unknown",
                    BookPublishedDate = ci.Book.PublishedDate.ToString("yyyy-MM-dd"),
                    BookPrice = ci.Book.Price.ToString(),
                    BookState = ci.Book.State.ToString(),
                    BookRating = ci.Book.Rating.ToString(),
                    BookReviewCount = ci.Book.ReviewCount.ToString(),
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Book.Price,
                    TotalPrice = ci.Quantity * ci.Book.Price
                }).ToList()
            };

            return new ResponseMVC<CartResponseDto>(200, "Cart found", cartResponse);
        }
        #endregion

        #region AddToCart
        public async Task<ResponseMVC<CartResponseDto>> AddToCart(AddToCartDto addToCartDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.Query()
                .Include(c => c.ShoppingCartItems)
                .FirstOrDefaultAsync(c => c.UserName == addToCartDto.UserName);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserName = addToCartDto.UserName,
                    UserId = addToCartDto.UserId,
                    CreatedAt = DateTime.UtcNow,
                    IsCheckedOut = false,
                    ShoppingCartItems = new List<CartItem>()
                };
                await _unitOfWork.ShoppingCartRepo.Add(cart);
            }

            var existingItem = cart.ShoppingCartItems
                .FirstOrDefault(ci => ci.BookId == addToCartDto.BookId);

            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                cart.ShoppingCartItems.Add(new CartItem 
                {
                    BookId = addToCartDto.BookId,
                    Quantity = addToCartDto.Quantity
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetCartByUserName(addToCartDto.UserName);
        }
        #endregion

        #region RemoveCartItem
        public async Task<ResponseMVC<CartResponseDto>> RemoveCartItem(RemoveCartItemDto removeCartItemDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.Query()
                .Include(c => c.ShoppingCartItems)
                .FirstOrDefaultAsync(c => c.UserName == removeCartItemDto.UserName);

            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }

            var item = cart.ShoppingCartItems.FirstOrDefault(ci => ci.CartItemId == removeCartItemDto.CartItemId);
            if (item == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Item not found in cart", null);
            }

            cart.ShoppingCartItems.Remove(item);
            await _unitOfWork.SaveChangesAsync();

            return await GetCartByUserName(removeCartItemDto.UserName);
        }
        #endregion

        #region UpdateCartItem
        public async Task<ResponseMVC<CartResponseDto>> UpdateCartItem(UpdateCartItemDto updateCartItemDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.Query()
                .Include(c => c.ShoppingCartItems)
                .FirstOrDefaultAsync(c => c.UserName == updateCartItemDto.UserName);

            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }

            var item = cart.ShoppingCartItems.FirstOrDefault(ci => ci.CartItemId == updateCartItemDto.CartItemId);
            if (item == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Item not found in cart", null);
            }

            item.Quantity = updateCartItemDto.Quantity;
            await _unitOfWork.SaveChangesAsync();

            return await GetCartByUserName(updateCartItemDto.UserName);
        }
        #endregion

        #region CheckoutAsync
        public async Task<ResponseMVC<CartResponseDto>> CheckoutAsync(CheckoutRequestDto checkoutRequestDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.Query()
                .Include(c => c.ShoppingCartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserName == checkoutRequestDto.UserName);

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart is empty", null);
            }

            // Example: Create order
            var order = new Order
            {
                UserName = checkoutRequestDto.UserName,
                OrderDate = DateTime.UtcNow,
                Address = checkoutRequestDto.Address,
                TotalAmount = cart.ShoppingCartItems.Sum(ci => ci.Quantity * ci.Book.Price),
                OrderItems = cart.ShoppingCartItems.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Book.Price
                }).ToList()
            };

            await _unitOfWork.OrderRepo.Add(order);

            // Mark cart as checked out
            cart.IsCheckedOut = true;
            cart.ShoppingCartItems.Clear();
            await _unitOfWork.SaveChangesAsync();

            return new ResponseMVC<CartResponseDto>(200, "Checkout successful", null);
        }
        #endregion
    }
}
