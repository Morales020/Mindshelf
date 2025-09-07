using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(decimal amount, int OrderId, string address = null, List<MindShelf_BL.Dtos.CartsDto.CartItemResponseDto> orderItems = null);
        Task<string> CreateCartCheckoutSessionAsync(decimal amount, int orderId, List<MindShelf_BL.Dtos.CartsDto.CartItemResponseDto> cartItems, string address = null);
    }
}
