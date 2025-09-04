using MindShelf_BL.Dtos.OrderDtos;
using MindShelf_BL.Helper;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
    public interface IOrderServices
    {
        Task<ResponseMVC<OrderResponseDto>> CreateOrderAsync(CreateOrderDto orderDto);
        Task<ResponseMVC<OrderResponseDto>> GetOrderByIdAsync(int orderId);
        Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync(int PageNumber, int PageSize);
        Task<ResponseMVC<bool>> UpdateOrderStatusAsync(int orderId, OrderState status);
        Task<ResponseMVC<bool>> DeleteOrderAsync(int orderId);
        Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByStatusAsync(OrderState status, int pageNumber, int pageSize);
        Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByUserNameAsync(string userId);
        Task<ResponseMVC<decimal>> CalculateTotalAmountAsync(int orderId);
        Task<ResponseMVC<OrderResponseDto>> CancelOrder(int orderId);
    }
}
