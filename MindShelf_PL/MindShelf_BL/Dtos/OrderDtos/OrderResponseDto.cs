using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.OrderDtos
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderState OrderStatus { get; set; } // e.g., Pending, Shipped, Delivered, Cancelled
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderItemResponseDto> OrderItems { get; set; } = [];
        public int ItemCount => OrderItems?.Count ?? 0;
    }
}
