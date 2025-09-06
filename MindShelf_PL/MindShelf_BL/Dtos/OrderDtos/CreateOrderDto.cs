using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Dtos.OrderDtos
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "UserName is required.")]
        [StringLength(100, ErrorMessage = "UserName cannot be longer than 100 characters.")]
        public string UserName { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "OrderItems are required.")]
        [MinLength(1, ErrorMessage = "At least one order item is required.")]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }
}
