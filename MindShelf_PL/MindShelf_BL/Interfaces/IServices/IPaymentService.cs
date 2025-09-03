using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_BL.Interfaces.IServices
{
   public interface IPaymentService
   {
       Task<string> CreateCheckoutSessionAsync(decimal amount, int OrderId);
   }
}
