using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShoppingCartId { get; set; }

        public bool IsCheckedOut { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public User User { get; set; }

        public ICollection<CartItem> ShoppingCartItems { get; set; } = new List<CartItem>();
    }
}
