using Microsoft.Extensions.DependencyInjection;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddScoped<IBookServies, BookServies>();
            services.AddScoped<ICartServices, CartServices>();
            services.AddScoped<IAuthorServies, AuthorServies>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IOrderServices, OrderServices>();
            services.AddScoped<IEventServices, EventServices>();
            services.AddScoped<IReviewServices, ReviewServices>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IFavouriteBookService, FavouriteBookService>();
            services.AddScoped<IEmailServies, EmailServices>();

            return services;
        }
    }
}
