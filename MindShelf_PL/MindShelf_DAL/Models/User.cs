using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class User : IdentityUser
    {
        public string Address { get; set; }
        public char Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ProfileImageUrl { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
        public ICollection<FavouriteBook> FavouriteBooks { get; set; } = new List<FavouriteBook>();
    }
}
