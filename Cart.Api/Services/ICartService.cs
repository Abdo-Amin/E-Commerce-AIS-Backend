using Cart.Api.Domain;
using Cart.Api.DTOs;

namespace Cart.Api.Services;

public interface ICartService
{
    Task<ShoppingCart> GetAsync(string userId, CancellationToken cancellationToken);

    Task<ShoppingCart> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken);

    Task<ShoppingCart?> UpdateItemAsync(
        string userId,
        int productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken);

    Task<ShoppingCart?> RemoveItemAsync(
        string userId,
        int productId,
        CancellationToken cancellationToken);

    Task DeleteAsync(string userId, CancellationToken cancellationToken);
}
