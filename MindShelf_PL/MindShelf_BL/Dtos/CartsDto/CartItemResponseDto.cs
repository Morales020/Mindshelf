namespace MindShelf_BL.Dtos.CartsDto;

public class CartItemResponseDto
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookName { get; set; }
    public string BookImageUrl { get; set; }
    public string BookDescription { get; set; }
    public string BookAuthor { get; set; }
    public string BookCategory { get; set; }
    public string BookPublishedDate { get; set; }
    public string BookPrice { get; set; }
    public string BookState { get; set; }
    public string BookRating { get; set; }
    public string BookReviewCount { get; set; }
}
