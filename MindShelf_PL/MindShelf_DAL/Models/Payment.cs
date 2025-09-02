using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindShelf_DAL.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentState State { get; set; } = PaymentState.Pending;

        public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;

        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }

        public Order Order { get; set; } = null!;
    }
    public enum PaymentState
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
    public enum PaymentMethod
    {
        CreditCard,
        PayPal,
        BankTransfer,
        CashOnDelivery
    }

}
