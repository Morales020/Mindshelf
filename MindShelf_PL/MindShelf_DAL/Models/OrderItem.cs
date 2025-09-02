using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindShelf_DAL.Models;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be positive.")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be positive.")]
    public decimal TotalPrice { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [ForeignKey(nameof(Book))]
    public int BookId { get; set; }
    public Book Book { get; set; }
}