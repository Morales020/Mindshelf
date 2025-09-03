using MindShelf_BL.Interfaces;
using MindShelf_BL.Repository;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.UnitWork
{
    public class UnitOfWork
    {
        public readonly MindShelfDbContext _dbcontext;
        IRepository<Book> _Books;//1
        IRepository<Category> _categories;//2
        IRepository<Order> _orders;//4
        IRepository<OrderItem> _orderItems;//5
        IRepository<Event> _Event;
        IRepository<Review> _reviews;
        IRepository<ShoppingCart> _shoppingCarts;
        IRepository<CartItem> _cartItems;
        IRepository<Payment> _payments;
        IRepository<FavouriteBook> _favourites;
        IRepository<EventRegistration > _eventRegistration;
        IRepository<Author> _Author;
       

        public UnitOfWork(MindShelfDbContext dBcontext)
        {
            _dbcontext = dBcontext;
        }




        #region BookRepo
        public IRepository<Book> BookRepo
        {
            get
            {
                if (_Books == null)
                {
                    _Books = new Repository<Book>(_dbcontext);
                }
                return _Books;
            }
        }
        #endregion

        #region CategorRepo
        public IRepository<Category> CategoryRepo
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new Repository<Category>(_dbcontext);
                }
                return _categories;
            }
        }

        #endregion

        #region OrderRepo
        public IRepository<Order> OrderRepo
        {
            get
            {
                if (_orders == null)
                {
                    _orders = new Repository<Order>(_dbcontext);
                }
                return _orders;
            }
        }
        #endregion

        #region OrderItemRepo
        public IRepository<OrderItem> OrderItemRepo
        {
            get
            {
                if (_orderItems == null)
                {
                    _orderItems = new Repository<OrderItem>(_dbcontext);
                }
                return _orderItems;
            }
        }
        #endregion

        #region EventRepo
        public IRepository<Event> EventRepo
        {
            get
            {
                if (_Event == null)
                {
                    _Event = new Repository<Event>(_dbcontext);
                }
                return _Event;
            }
        }
        #endregion

        #region ReviewRepo
        public IRepository<Review> ReviewRepo
        {
            get
            {
                if (_reviews == null)
                {
                    _reviews = new Repository<Review>(_dbcontext);
                }
                return _reviews;
            }
        }
        #endregion

        #region ShoppingCartRepo
        public IRepository<ShoppingCart> ShoppingCartRepo
        {
            get
            {
                if (_shoppingCarts == null)
                {
                    _shoppingCarts = new Repository<ShoppingCart>(_dbcontext);
                }
                return _shoppingCarts;
            }
        }
        #endregion

        #region CartItemRepo
        public IRepository<CartItem> CartItemRepo
        {
            get
            {
                if (_cartItems == null)
                {
                    _cartItems = new Repository<CartItem>(_dbcontext);
                }
                return _cartItems;
            }
        }
        #endregion

        # region PaymentRepo
        public IRepository<Payment> PaymentRepo
        {
            get
            {
                if (_payments == null)
                {
                    _payments = new Repository<Payment>(_dbcontext);
                }
                return _payments;
            }
        }
        #endregion

        # region FavoriteBookRepo
        public IRepository<FavouriteBook> FavoriteBookRepo
        {
            get
            {
                if(_favourites == null)
                {
                    _favourites= new Repository<FavouriteBook>(_dbcontext);
                }
                return _favourites;

            }
        }
        #endregion

        #region AuthorRepo
        public IRepository<Author> AuthorRepo
        {
            get
            {
                if (_Author == null) 
                {
                    _Author = new Repository<Author>(_dbcontext);

                }
                return _Author;
            }
        }
        #endregion

       #region EventRegistrationRepo
        public IRepository<EventRegistration> EventRegistrationRepo
        {
            get
            {
                if(_eventRegistration== null)
                {
                    _eventRegistration= new Repository<EventRegistration>(_dbcontext);
                }
                return _eventRegistration;
            }
        }
        #endregion
        




        public async Task<int> SaveChangesAsync()
        {
            return await _dbcontext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbcontext.Dispose();
        }
    }
}
