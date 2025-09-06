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

        #region GetOrderById
        public async Task<ResponseMVC<OrderResponseDto>> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.OrderRepo.Query()
                    .AsNoTracking()
                    .Where(o => o.OrderId == orderId)
                    .Select(o => new OrderResponseDto
                    {
                        Id = o.OrderId,
                        UserId = o.UserId,
                        UserName = o.UserName,
                        ShippingAddress = o.Address,
                        OrderDate = o.OrderDate,
                        OrderStatus = o.State,
                        TotalAmount = o.TotalAmount,
                        OrderItems = o.OrderItems.Select(item => new OrderItemResponseDto
                        {
                            OrderItemId = item.OrderItemId,
                            BookId = item.BookId,
                            Quantity = item.Quantity,
                            TotalPrice = item.TotalPrice
                        }).ToList()
                    }).FirstOrDefaultAsync();

                if (order == null)
                {
                    return new ResponseMVC<OrderResponseDto>
                    {
                        StatusCode = 404,
                        Message = "Order not found.",
                        Data = default
                    };
                }

                return new ResponseMVC<OrderResponseDto>
                {
                    StatusCode = 200,
                    Message = "Order retrieved successfully.",
                    Data = order
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<OrderResponseDto>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = default
                };
            }
        }
        #endregion

        #region GetAllOrder
        public async Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync(string? userId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10
            ) 
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _unitOfWork.OrderRepo.Query()
                .Include(o => o.OrderItems)
                .AsNoTracking();

            
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.UserId == userId);
            }

            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderState>(status, out var state))
            {
                query = query.Where(o => o.State == state);
            }

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            var totalOrders = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderResponseDto
                {
                    Id = o.OrderId,
                    OrderStatus = o.State,
                    TotalAmount = o.TotalAmount,
                    UserId = o.UserId,
                    UserName = o.UserName,
                    ShippingAddress = o.Address,
                    OrderDate = o.OrderDate,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderItemId = oi.OrderItemId,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToListAsync();

            return new ResponseMVC<IEnumerable<OrderResponseDto>>
            {
                StatusCode = 200,
                Message = orders.Any() ? "Orders retrieved successfully." : "No orders found.",
                Data = orders,
                TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize)
            };
        }
        #endregion



        #region UpdateOrderStauts
        public async Task<ResponseMVC<bool>> UpdateOrderStatusAsync(int orderId, OrderState status)
        {
            try
            {
                var order = await _unitOfWork.OrderRepo.Query().FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    return new ResponseMVC<bool>
                    {
                        StatusCode = 404,
                        Message = "Order not found.",
                        Data = false
                    };
                }
                order.State = status;
                _unitOfWork.OrderRepo.Update(order);
                await _unitOfWork.SaveChangesAsync();
                return new ResponseMVC<bool>
                {
                    StatusCode = 200,
                    Message = "Order status updated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<bool>
                {
                    StatusCode = 500,
                    Message = $"An error occurred while updating the order status.{ex.Message}",
                    Data = false
                };
            }
        }

        #endregion

        #region DeleteOrder
        public async Task<ResponseMVC<bool>> DeleteOrderAsync(int orderId)
        {

            try
            {
                var order = await _unitOfWork.OrderRepo.Query().FirstOrDefaultAsync(c => c.OrderId == orderId);
                if (order == null)
                {
                    return new ResponseMVC<bool>
                    {
                        StatusCode = 404,
                        Message = " order not found",
                        Data = false
                    };
                }
                await _unitOfWork.OrderRepo.Delete(order.OrderId).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                return new ResponseMVC<bool>
                {
                    StatusCode = 200,
                    Message = " order Deleted",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<bool>
                {
                    StatusCode = 500,
                    Message = ex.Message,
                    Data = false
                };
            }
        }
        #endregion

        #region GetOrderByStatus
        public async Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByStatusAsync(OrderState status, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var totalNumber = await _unitOfWork.OrderRepo.Query()
                .Where(o => o.State == status)
                .CountAsync();

            var orders = await _unitOfWork.OrderRepo.Query()
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.State == status)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderResponseDto
                {
                    Id = o.OrderId,
                    OrderStatus = o.State,
                    TotalAmount = o.TotalAmount,
                    UserId = o.UserId,
                    UserName = o.UserName,
                    ShippingAddress = o.Address,
                    OrderDate = o.OrderDate,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        OrderItemId = oi.OrderItemId,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        BookName = oi.Book.Title,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToListAsync();

            return new ResponseMVC<IEnumerable<OrderResponseDto>>
            {
                StatusCode = 200,
                Message = orders.Any() ? "Orders retrieved successfully." : "No orders found with the specified status.",
                Data = orders,
                TotalPages = (int)Math.Ceiling((double)totalNumber / pageSize)
            };
        }

        #endregion

        #region GetOrderBYuserName
        public async Task<ResponseMVC<IEnumerable<OrderResponseDto>>> GetOrdersByUserNameAsync(string userName)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return new ResponseMVC<IEnumerable<OrderResponseDto>>
                    {
                        StatusCode = 404,
                        Message = "User not found.",
                        Data = new List<OrderResponseDto>()
                    };
                }

                var orders = await _unitOfWork.OrderRepo.Query()
                    .Include(o => o.OrderItems)
                    .AsNoTracking()
                    .Where(o => o.UserId == user.Id)
                    .Select(o => new OrderResponseDto
                    {
                        OrderStatus = o.State,
                        TotalAmount = o.TotalAmount,
                        UserId = o.UserId,
                        UserName = o.UserName,
                        ShippingAddress = o.Address,
                        OrderDate = o.OrderDate,
                        OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                        {
                            OrderItemId = oi.OrderItemId,
                            BookName = oi.Book.Title, 
                            TotalPrice = oi.TotalPrice
                        }).ToList()
                    }).ToListAsync();

                return new ResponseMVC<IEnumerable<OrderResponseDto>>
                {
                    StatusCode = 200,
                    Message = orders.Any() ? "Orders retrieved successfully." : "No orders found for this user.",
                    Data = orders
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<IEnumerable<OrderResponseDto>>
                {
                    StatusCode = 500,
                    Message = $"An error occurred while retrieving orders: {ex.Message}",
                    Data = new List<OrderResponseDto>() 
                };
            }
        }
        #endregion

        #region CalcultateTotalAmount
        public async Task<ResponseMVC<decimal>> CalculateTotalAmountAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.OrderRepo.Query()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return new ResponseMVC<decimal>
                    {
                        StatusCode = 404,
                        Message = "Order not found.",
                        Data = 0
                    };
                }

                var totalAmount = order.OrderItems?.Sum(oi => oi.TotalPrice) ?? 0;

                if (order.TotalAmount != totalAmount)
                {
                    order.TotalAmount = totalAmount;
                    await _unitOfWork.SaveChangesAsync();
                }

                return new ResponseMVC<decimal>
                {
                    StatusCode = 200,
                    Message = "Total amount retrieved successfully.",
                    Data = totalAmount
                };
            }
            catch (Exception ex)
            {
                return new ResponseMVC<decimal>
                {
                    StatusCode = 500,
                    Message = $"An error occurred: {ex.Message}",
                    Data = 0
                };
            }
        }

        #endregion

        #region CancelOrder
        public async Task<ResponseMVC<OrderResponseDto>> CancelOrder(int orderId)
        {
            var order = await _unitOfWork.OrderRepo.Query()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return new ResponseMVC<OrderResponseDto>(404, "Order not found");
            }


            if (!(order.State == OrderState.Pending || order.State == OrderState.Processing))
            {
                return new ResponseMVC<OrderResponseDto>(400, "Order cannot be cancelled at this stage");
            }


            foreach (var item in order.OrderItems)
            {
                item.Book.Stock += item.Quantity;
                _unitOfWork.BookRepo.Update(item.Book);
            }


            order.State = OrderState.Cancelled;
            _unitOfWork.OrderRepo.Update(order);

            await _unitOfWork.SaveChangesAsync();

            return new ResponseMVC<OrderResponseDto>(200, "Order cancelled successfully and stock restored",
                new OrderResponseDto
                {
                    Id = order.OrderId,
                    OrderStatus = order.State
                });
        }
        #endregion
    }
}
