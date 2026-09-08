namespace Cart.Api.Services;

public interface IProductCatalogClient
{
    Task<ProductCatalogItem?> GetByIdAsync(int productId, CancellationToken cancellationToken);
}
