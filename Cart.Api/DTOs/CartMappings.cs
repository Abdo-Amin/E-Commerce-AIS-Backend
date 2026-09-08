using Cart.Api.Domain;

namespace Cart.Api.DTOs;

public static class CartMappings
{
    public static CartResponse ToResponse(this ShoppingCart cart)
    {
        var items = cart.Items
            .Select(item => new CartItemResponse
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.UnitPrice * item.Quantity
            })
            .ToList();

        return new CartResponse
        {
            UserId = cart.UserId,
            Items = items,
            TotalItems = items.Sum(item => item.Quantity),
            TotalPrice = items.Sum(item => item.LineTotal),
            UpdatedAt = cart.UpdatedAt
        };
    }
}
