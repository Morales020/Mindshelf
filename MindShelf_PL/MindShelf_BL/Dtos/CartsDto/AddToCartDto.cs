using System.ComponentModel.DataAnnotations;

namespace MindShelf_BL.Dtos.CartsDto;

public class AddToCartDto
{
    public string UserId;

    public String UserName { get; set; }

    [Required(ErrorMessage = "ProductId is required.")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Quantity is required."), Range(1, 100, ErrorMessage = "must be between 1 to 100")]
    public int Quantity { get; set; }
}
