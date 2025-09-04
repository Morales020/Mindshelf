using System.ComponentModel.DataAnnotations;
namespace MindShelf_BL.Dtos.CartsDto;

public class UpdateCartItemDto
{
    [Required(ErrorMessage = "UserName is required.")]
    public String UserName { get; set; }

    [Required(ErrorMessage = "CartItemId is required.")]
    public int CartItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required."), Range(1, 100, ErrorMessage = "must be between 1 to 100")]
    public int Quantity { get; set; }
}
