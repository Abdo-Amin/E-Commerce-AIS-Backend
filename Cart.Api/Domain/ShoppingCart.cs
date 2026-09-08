namespace Cart.Api.Domain;

public class ShoppingCart
{
    public string UserId { get; set; } = string.Empty;

    public List<CartItem> Items { get; set; } = [];

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
