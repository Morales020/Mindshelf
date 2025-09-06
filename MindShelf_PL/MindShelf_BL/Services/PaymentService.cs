using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using Stripe.Checkout;
using Microsoft.Extensions.Options;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models.Stripe;
using Stripe.Climate;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using MindShelf_BL.Dtos.CartsDto;
namespace MindShelf_BL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly StripeSettings _stripeSettings;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(UnitOfWork unitOfWork, IOptions<StripeSettings> stripeSettings, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeSettings.Value;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            Stripe.StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        public async Task<string> CreateCheckoutSessionAsync(decimal amount, int orderId)
        {
            var order = await _unitOfWork.OrderRepo.GetById(orderId);
            // Fix: Use HttpContext instead of ClaimsPrincipal.Current
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user?.Identity?.Name;

            if (order == null)
            {
                order = new MindShelf_DAL.Models.Order
                {
                    UserId = userId,
                    UserName = userName,
                    OrderDate = DateTime.UtcNow,
                    State = OrderState.Pending,
                    Address = "N/A",
                    TotalAmount = amount,
                    Discount = 0
                };

                await _unitOfWork.OrderRepo.Add(order);
                await _unitOfWork.SaveChangesAsync();
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                   {
                       new SessionLineItemOptions
                       {
                           PriceData = new SessionLineItemPriceDataOptions
                           {
                               Currency = "usd",
                               UnitAmount = (long)(amount * 100),
                               ProductData = new SessionLineItemPriceDataProductDataOptions
                               {
                                   Name = $"Order #{order.OrderId}"
                               }
                           },
                           Quantity = 1
                       }
                   },
                Mode = "payment",
                SuccessUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/Payment/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/Payment/Cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var payment = new Payment
            {
                Amount = amount,
                OrderId = order.OrderId,
                PaymentDate = DateTime.UtcNow,
                TransactionId = session.Id,
                State = PaymentState.Pending,
                Method = PaymentMethod.CreditCard,
            };

            await _unitOfWork.PaymentRepo.Add(payment);
            await _unitOfWork.SaveChangesAsync();

            return session.Id;
        }
        // Add new method for cart-based checkout
        public async Task<string> CreateCartCheckoutSessionAsync(decimal amount, int orderId, List<CartItemResponseDto> cartItems)
        {
            var order = await _unitOfWork.OrderRepo.GetById(orderId);

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = user?.Identity?.Name;

            if (order == null)
            {
                order = new MindShelf_DAL.Models.Order
                {
                    UserId = userId,
                    UserName = userName,
                    OrderDate = DateTime.UtcNow,
                    State = OrderState.Pending,
                    Address = "N/A",
                    TotalAmount = amount,
                    Discount = 0
                };

                await _unitOfWork.OrderRepo.Add(order);
                await _unitOfWork.SaveChangesAsync();
            }

            // Create line items from cart items
            var lineItems = cartItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = (long)(item.BookId * 100), // Convert to cents
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.BookName,
                        Description = item.BookAuthor
                    }
                },
                Quantity = item.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = "https://localhost:7099/Payment/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:7099/Payment/Cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var payment = new Payment
            {
                Amount = amount,
                OrderId = order.OrderId,
                PaymentDate = DateTime.UtcNow,
                TransactionId = session.Id,
                State = PaymentState.Pending,
                Method = PaymentMethod.CreditCard,
            };

            await _unitOfWork.PaymentRepo.Add(payment);
            await _unitOfWork.SaveChangesAsync();

            return session.Id;
        }
    }
}
