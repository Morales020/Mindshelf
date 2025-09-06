using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Helper.SeedData;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using MindShelf_DAL.Models.Stripe;
using Stripe;
using System;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MindShelf_PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load .env file
            var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env");
            if (File.Exists(envFilePath))
            {
                foreach (var line in System.IO.File.ReadAllLines(envFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                    }
                }
            }

            // Add environment variables to configuration
            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<MindShelfDbContext>(options =>
            options.UseSqlServer(
             builder.Configuration.GetConnectionString("Cs"),
             sqlOptions => sqlOptions.EnableRetryOnFailure(
              maxRetryCount: 5,                        
              maxRetryDelay: TimeSpan.FromSeconds(10), 
              errorNumbersToAdd: null
         )
     )
 );

            builder.Services.AddIdentity<User,IdentityRole>().AddEntityFrameworkStores<MindShelfDbContext>();

            // Configure Stripe Settings
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddScoped<UnitOfWork>();
            // add services here
            builder.Services.AddScoped<IBookServies,BookServies>();
            builder.Services.AddScoped<ICartServices,CartServices>();
            builder.Services.AddScoped<IAuthorServies, AuthorServies>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderServices, OrderServices>();
            builder.Services.AddScoped<IEventServices, EventServices>();
            builder.Services.AddScoped<IReviewServices, ReviewServices>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();


            //builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            //StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

            // Stripe Settings
            builder.Services.Configure<StripeSettings>(options =>
            {
                options.SecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                                    ?? builder.Configuration["Stripe:SecretKey"];

                options.PublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY")
                                         ?? builder.Configuration["Stripe:PublishableKey"];
            });

            // Set global Stripe API key
            var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                                  ?? builder.Configuration["Stripe:SecretKey"];

            if (string.IsNullOrEmpty(stripeSecretKey))
            {
                throw new Exception("Stripe Secret Key is missing. Please check your .env file.");
            }

            StripeConfiguration.ApiKey = stripeSecretKey;

            //Google Settings
            builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                           ?? builder.Configuration["Authentication:Google:ClientId"];

        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                               ?? builder.Configuration["Authentication:Google:ClientSecret"];

        options.CallbackPath = "/signin-google";
    });


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

            app.UseAuthentication();
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
