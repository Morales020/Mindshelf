using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
        public class Order
        {
            [Key]
            public int OrderId { get; set; }
            [Required]
            public DateTime OrderDate { get; set; }
            [Required]
            public string UserId { get; set; }
            [Required]
            public string UserName { get; set; }
            [Column(TypeName = "decimal(18,2)")]
            public decimal TotalAmount { get; set; }
            [Column(TypeName = "decimal(18,2)")]
            public decimal Discount { get; set; }
            [Required]
            public string Address { get; set; }
            public OrderState State { get; set; } = OrderState.Pending;

            public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        }

        public enum OrderState
        {
            Pending,
            Processing,
            Confirmed,
            Cancelled,
            Shipping,
            Returned,
            Delivered
        }
}
