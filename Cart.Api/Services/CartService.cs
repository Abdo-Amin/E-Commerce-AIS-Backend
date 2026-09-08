using System.Text.Json;
using Cart.Api.Domain;
using Cart.Api.DTOs;
using StackExchange.Redis;

namespace Cart.Api.Services;

public class CartService : ICartService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan CartExpiration = TimeSpan.FromDays(7);
    private readonly IProductCatalogClient _productCatalogClient;
    private readonly IDatabase _redis;

    public CartService(
        IConnectionMultiplexer redis,
        IProductCatalogClient productCatalogClient)
    {
        _redis = redis.GetDatabase();
        _productCatalogClient = productCatalogClient;
    }

    public async Task<ShoppingCart> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await GetCartFromRedisAsync(userId);

        return cart ?? new ShoppingCart
        {
            UserId = userId
        };
    }

    public async Task<ShoppingCart> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        ValidateQuantity(request.Quantity);

        var product = await GetProductOrThrowAsync(request.ProductId, cancellationToken);
        var cart = await GetAsync(userId, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == request.ProductId);
        var requestedQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

        EnsureStockIsAvailable(product, requestedQuantity);

        if (existingItem is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = GetPrimaryImageUrl(product),
                UnitPrice = product.Price,
                Quantity = request.Quantity
            });
        }
        else
        {
            existingItem.ProductName = product.Name;
            existingItem.ImageUrl = GetPrimaryImageUrl(product);
            existingItem.UnitPrice = product.Price;
            existingItem.Quantity = requestedQuantity;
        }

        await SaveAsync(cart);

        return cart;
    }

    public async Task<ShoppingCart?> UpdateItemAsync(
        string userId,
        int productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        ValidateQuantity(request.Quantity);

        var cart = await GetCartFromRedisAsync(userId);
        var item = cart?.Items.FirstOrDefault(cartItem => cartItem.ProductId == productId);

        if (cart is null || item is null)
            return null;

        var product = await GetProductOrThrowAsync(productId, cancellationToken);

        EnsureStockIsAvailable(product, request.Quantity);

        item.ProductName = product.Name;
        item.ImageUrl = GetPrimaryImageUrl(product);
        item.UnitPrice = product.Price;
        item.Quantity = request.Quantity;

        await SaveAsync(cart);

        return cart;
    }

    public async Task<ShoppingCart?> RemoveItemAsync(
        string userId,
        int productId,
        CancellationToken cancellationToken)
    {
        var cart = await GetCartFromRedisAsync(userId);

        if (cart is null)
            return null;

        var removed = cart.Items.RemoveAll(item => item.ProductId == productId) > 0;

        if (!removed)
            return null;

        await SaveAsync(cart);

        return cart;
    }

    public async Task DeleteAsync(string userId, CancellationToken cancellationToken)
    {
        await _redis.KeyDeleteAsync(GetKey(userId));
    }

    private async Task<ShoppingCart?> GetCartFromRedisAsync(string userId)
    {
        var value = await _redis.StringGetAsync(GetKey(userId));

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<ShoppingCart>(value.ToString(), JsonOptions);
    }

    private async Task SaveAsync(ShoppingCart cart)
    {
        cart.UpdatedAt = DateTime.UtcNow;

        var value = JsonSerializer.Serialize(cart, JsonOptions);

        await _redis.StringSetAsync(
            GetKey(cart.UserId),
            value,
            CartExpiration);
    }

    private async Task<ProductCatalogItem> GetProductOrThrowAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var product = await _productCatalogClient.GetByIdAsync(productId, cancellationToken);

        return product
            ?? throw new InvalidOperationException($"Product with id '{productId}' was not found.");
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
    }

    private static void EnsureStockIsAvailable(ProductCatalogItem product, int requestedQuantity)
    {
        if (requestedQuantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                $"Only {product.StockQuantity} item(s) are available for product '{product.Id}'.");
        }
    }

    private static string? GetPrimaryImageUrl(ProductCatalogItem product)
    {
        return product.Images.FirstOrDefault(image => image.IsPrimary)?.ImageUrl
            ?? product.Images.FirstOrDefault()?.ImageUrl;
    }

    private static string GetKey(string userId)
    {
        return $"cart:user:{userId}";
    }
}
