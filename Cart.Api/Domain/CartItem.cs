namespace Cart.Api.Domain;

public class CartItem
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
}
