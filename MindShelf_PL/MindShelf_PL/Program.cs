using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindShelf_BL.Extensions;
using MindShelf_BL.Helper.SeedData;
using MindShelf_BL.Interfaces.IServices;
using MindShelf_BL.Services;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Data;
using MindShelf_DAL.Models;
using MindShelf_DAL.Models.Stripe;
using MindShelf_PL.Hubs;
using Stripe;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MindShelf_PL
{
    public class Program
    {
        // Use NameIdentifier claim as SignalR user id for Clients.User(userId)
        private sealed class NameIdUserIdProvider : IUserIdProvider
        {
            public string? GetUserId(HubConnectionContext connection)
            {
                return connection?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }
        }
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
            builder.Services.AddSignalR(o => { 
                o.EnableDetailedErrors = true; 
            });
            // Ensure SignalR knows how to map a user to connections for Clients.User()
            builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, NameIdUserIdProvider>();
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

            // HttpContext accessor for services
            builder.Services.AddHttpContextAccessor();

            // Identity (registers UserManager<User>, SignInManager<User>, etc.)
            builder.Services
                .AddIdentity<User, IdentityRole>(options => { })
                .AddEntityFrameworkStores<MindShelfDbContext>()
                .AddDefaultTokenProviders();

            // Configure Stripe Settings
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddScoped<UnitOfWork>();
            
            builder.Services.AddBusinessLogicServices();

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
        options.AccessDeniedPath = "/Account/Login";
    })
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                           ?? builder.Configuration["Authentication:Google:ClientId"];

        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                               ?? builder.Configuration["Authentication:Google:ClientSecret"];

        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Events.OnCreatingTicket = context =>
        {
            try
            {
                var user = context.User;
                var name = user.TryGetProperty("name", out var n) ? n.GetString() : null;
                var picture = user.TryGetProperty("picture", out var p) ? p.GetString() : null;

                if (!string.IsNullOrWhiteSpace(name))
                    context.Identity!.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, name));
                if (!string.IsNullOrWhiteSpace(picture))
                    context.Identity!.AddClaim(new System.Security.Claims.Claim("urn:google:picture", picture));
            }
            catch { }
            return Task.CompletedTask;
        };
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

            // Support running behind reverse proxies (X-Forwarded-*)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<MindShelf_PL.Hubs.CommunityHub>("/communityHub", options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            });

            app.MapHub<MindShelf_PL.Hubs.BookNotificationHub>("/bookNotificationHub", options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            });

            app.MapHub<MindShelf_PL.Hubs.PrivateChatHub>("/privateChatHub", options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            });


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
