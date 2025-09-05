using MindShelf_BL.Interfaces.IServices;
using MindShelf_DAL.Models;
using Stripe.Checkout;
using Microsoft.Extensions.Options;
using MindShelf_BL.UnitWork;
using MindShelf_DAL.Models.Stripe;
using Stripe.Climate;

namespace MindShelf_BL.Services
{
   public class PaymentService : IPaymentService
   {
       private readonly UnitOfWork _unitOfWork;
       private readonly StripeSettings _stripeSettings;

       public PaymentService(UnitOfWork unitOfWork, IOptions<StripeSettings> stripeSettings)
       {
           _unitOfWork = unitOfWork;
           _stripeSettings = stripeSettings.Value;
           Stripe.StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
       }

       public async Task<string> CreateCheckoutSessionAsync(decimal amount, int orderId)
       {
           var order = await _unitOfWork.OrderRepo.GetById(orderId);

           if (order == null)
           {
               order = new MindShelf_DAL.Models.Order
               {
                   UserId = "51a01f86-e12c-448b-bc56-645092269389",
                   UserName = "admin",
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
