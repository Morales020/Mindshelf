using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.CartsDto;

public class AddToCartDto
{
    [Required(ErrorMessage = "UserName is required.")]
    public String UserName { get; set; }

    [Required(ErrorMessage = "ProductId is required.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required."), Range(1, 100, ErrorMessage = "must be between 1 to 100")]
    public int Quantity { get; set; }
}
