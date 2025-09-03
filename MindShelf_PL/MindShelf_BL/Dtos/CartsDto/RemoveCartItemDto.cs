using System.ComponentModel.DataAnnotations;
namespace MindShelf_BL.Dtos.CartsDto;

public class RemoveCartItemDto
{
    [Required(ErrorMessage = "UserName is required.")]
    public String UserName { get; set; }

    [Required(ErrorMessage = "CartItemId is required.")]
    public int CartItemId { get; set; }
}
