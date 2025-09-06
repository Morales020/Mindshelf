using MindShelf_BL.Dtos.CartsDto;
using MindShelf_BL.Helper;

namespace MindShelf_BL.Interfaces.IServices;

public interface ICartServices
{
    // get cart by user name
    Task<ResponseMVC<CartResponseDto>> GetCartByUserName(string userName);
    // add to cart
    Task<ResponseMVC<CartResponseDto>> AddToCart(AddToCartDto addToCartDto);
    // remove cart item
    Task<ResponseMVC<CartResponseDto>> RemoveCartItem(RemoveCartItemDto removeCartItemDto);
    // update cart item
    Task<ResponseMVC<CartResponseDto>> UpdateCartItem(UpdateCartItemDto updateCartItemDto);
    // checkout
    Task<ResponseMVC<CartResponseDto>> CheckoutAsync(CheckoutRequestDto checkoutRequestDto);
    // clear cart
    Task<ResponseMVC<CartResponseDto>> ClearCart(string userName);
    // get cart count
    Task<int> GetCartCount(string userName);
    // get cart total
    Task<decimal> GetCartTotal(string userName);
}
