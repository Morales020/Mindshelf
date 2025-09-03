using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Dtos.OrderDtos;
using MindShelf_BL.Helper;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models;

namespace MindShelf_BL.Services
{
    public class OrderServices : IOrderServices
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public OrderServices(UnitOfWork unitOfWork, UserManager<User> user)
        {
            _unitOfWork = unitOfWork;
            _userManager = user;
        }
        #region CreateOrder
        public async Task<ResponseMVC<OrderResponseDto>> CreateOrderAsync(CreateOrderDto orderDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(orderDto.UserName);
                if (user == null)
                {
                    return new ResponseMVC<OrderResponseDto>
                    {
                        StatusCode = 404,
                        Message = "User not found.",
                        Data = null
                    };
                }

                var shippingAddress = await _unitOfWork.OrderRepo.GetById(orderDto.ShippingAddressId);
                if (shippingAddress == null)
                {
                    return new ResponseMVC<OrderResponseDto>
                    {
                        StatusCode = 400,
                        Message = "Shipping address is invalid.",
                        Data = null
                    };
                }

                var bookIds = orderDto.OrderItems.Select(i => i.BookId).ToList();
                var books = await _unitOfWork.BookRepo.Query()
                    .Where(b => bookIds.Contains(b.BookId))
                    .ToListAsync();

                if (books.Count != bookIds.Count)
                {
                    return new ResponseMVC<OrderResponseDto>
                    {
                        StatusCode = 404,
                        Message = "One or more books not found.",
                        Data = null
                    };
                }

                foreach (var item in orderDto.OrderItems)
                {
                    var book = books.FirstOrDefault(b => b.BookId == item.BookId);
                    if (book == null || book.State != BookState.Available || book.Price <= 0)
                    {
                        return new ResponseMVC<OrderResponseDto>
                        {
                            StatusCode = 400,
                            Message = $"Book {book?.Title ?? "Unknown"} is not available or invalid price.",
                            Data = null
                        };
                    }
                }

                var order = new Order
                {
                    UserId = user.Id,
                    UserName = orderDto.UserName,
                    OrderDate = DateTime.UtcNow,
                    State = OrderState.Pending,
                    TotalAmount = 0,
                    Discount = 0,
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;
                foreach (var item in orderDto.OrderItems)
                {
                    var book = books.First(b => b.BookId == item.BookId);

                    var orderItem = new OrderItem
                    {
                        BookId = book.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = book.Price,
                        TotalPrice = book.Price * item.Quantity
                    };

                    totalAmount += orderItem.TotalPrice;
                    order.OrderItems.Add(orderItem);
                }

                order.TotalAmount = totalAmount;

                await _unitOfWork.OrderRepo.Add(order);
                await _unitOfWork.SaveChangesAsync();

                var orderResponse = new OrderResponseDto
                {
                    Id = order.OrderId,
                    UserId = order.UserId,
                    UserName = order.UserName,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.State,
                    TotalAmount = order.TotalAmount,
                    Discount = order.Discount,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderItemId = oi.OrderItemId,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        Price = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity,
                        BookName = books.FirstOrDefault(b => b.BookId == oi.BookId)?.Title
                    }).ToList()
                };

                return new ResponseMVC<OrderResponseDto>
                {
                    StatusCode = 201,
                    Message = "Order created successfully.",
                    Data = orderResponse
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<OrderResponseDto>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
        #endregion

        public Task<ResponseMVC<OrderResponseDto>> GetOrderByIdAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync(int PageNumber, int PageSize)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<bool>> UpdateOrderStatusAsync(int orderId, OrderState status)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<bool>> DeleteOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByStatusAsync(OrderState status, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByUserNameAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<decimal>> CalculateTotalAmountAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseMVC<OrderResponseDto>> CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }
    }
}
