namespace Cart.Api.DTOs;

public class CartResponse
{
    public string UserId { get; set; } = string.Empty;

    public IReadOnlyList<CartItemResponse> Items { get; set; } = [];

    public int TotalItems { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime UpdatedAt { get; set; }
}
