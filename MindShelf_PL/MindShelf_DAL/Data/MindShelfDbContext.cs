using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MindShelf_DAL.Models;

namespace MindShelf_DAL.Data
{
    public class MindShelfDbContext : IdentityDbContext<MindShelf_DAL.Models.User>
    {
        public MindShelfDbContext(DbContextOptions<MindShelfDbContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<FavouriteBook> FavouriteBooks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Payment>()
                .Property(p => p.Method)
                .HasConversion<string>();
            builder.Entity<Payment>()
                .Property(p => p.State)
                .HasConversion<string>();
            builder.Entity<Order>()
                .Property(o => o.State)
                .HasConversion<string>();
            builder.Entity<Book>()
                .Property(b => b.State)
                .HasConversion<string>();
        }

    }
}
