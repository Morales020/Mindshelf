using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Helper.SeedData;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using System.Threading.Tasks;

namespace MindShelf_PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<MindShelfDbContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("Cs")));
            builder.Services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<MindShelfDbContext>();

            builder.Services.AddScoped<UnitOfWork>();
            // add services here
            builder.Services.AddScoped<IBookServies,BookServies>();
            builder.Services.AddScoped<ICartServices,CartServices>();






            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scop = app.Services.CreateScope())
            {
                var servies= scop.ServiceProvider;
                try
                {
                    await AddUser.Initiez(servies);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while initializing the application: {ex.Message}");
                }
            }


            app.Run();
        }
    }
}
