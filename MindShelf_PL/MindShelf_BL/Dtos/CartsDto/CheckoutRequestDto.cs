using System.ComponentModel.DataAnnotations;
namespace MindShelf_BL.Dtos.CartsDto;

public class CheckoutRequestDto
{
    [Required(ErrorMessage = "UserName is required.")]
    public String UserName { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string Address { get; set; }

    [Required(ErrorMessage = "PaymentMethod is required.")]
    public string PaymentMethod { get; set; }
}
