using MindShelf_BL.UnitWork;
using MindShelf_BL.Helper;
using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;

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
            var cart = await _unitOfWork.ShoppingCartRepo.Query().Where(c => c.UserName == userName).ToListAsync();
            if (cart == null || !cart.Any())
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }
            
            // Convert to CartResponseDto
            var cartResponse = new CartResponseDto
            {
                UserName = userName,
                CartItems = cart.SelectMany(c => c.ShoppingCartItems).Select(ci => new CartItemResponseDto
                {
                    BookName = ci.Book.Title,
                    BookImageUrl = ci.Book.ImageUrl,
                    BookDescription = ci.Book.Description,
                    BookAuthor = ci.Book.Author.Name,
                    BookCategory = ci.Book.Category.Name,
                    BookPublishedDate = ci.Book.PublishedDate.ToString(),
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
            var cart = await _unitOfWork.ShoppingCartRepo.GetAll();
            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }
            return new ResponseMVC<CartResponseDto>(200, "Cart found", null);
        }
        #endregion
        #region RemoveCartItem
        public async Task<ResponseMVC<CartResponseDto>> RemoveCartItem(RemoveCartItemDto removeCartItemDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.GetAll();
            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }
            return new ResponseMVC<CartResponseDto>(200, "Cart found", null);
        }
        #endregion
        #region UpdateCartItem
        public async Task<ResponseMVC<CartResponseDto>> UpdateCartItem(UpdateCartItemDto updateCartItemDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.GetAll();
            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }
            return new ResponseMVC<CartResponseDto>(200, "Cart found", null);
        }
        #endregion
        #region CheckoutAsync
        public async Task<ResponseMVC<CartResponseDto>> CheckoutAsync(CheckoutRequestDto checkoutRequestDto)
        {
            var cart = await _unitOfWork.ShoppingCartRepo.GetAll();
            if (cart == null)
            {
                return new ResponseMVC<CartResponseDto>(404, "Cart not found", null);
            }
            return new ResponseMVC<CartResponseDto>(200, "Cart found", null);
        }
        #endregion
    }
}
