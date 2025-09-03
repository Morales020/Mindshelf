namespace MindShelf_BL.Dtos.CartsDto;

public class CartResponseDto
{
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCheckedOut { get; set; }
    public ICollection<CartItemResponseDto> CartItems { get; set; } = new List<CartItemResponseDto>();

    
}
