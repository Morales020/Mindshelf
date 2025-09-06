using MindShelf_DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.CartsDto;

public class CheckoutRequestDto
{
    [Required(ErrorMessage = "UserName is required.")]
    public String UserName { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; }

    [Required(ErrorMessage = "PaymentMethod is required.")]
    public PaymentMethod PaymentMethod { get; set; }

    // Additional properties for Stripe integration
    public string Cardname { get; set; }
    public string Email { get; set; }
    public List<CartItemResponseDto> OrderItems { get; set; }
}
