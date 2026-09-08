using System.Net;
using System.Text.Json;

namespace Cart.Api.Services;

public class ProductCatalogClient : IProductCatalogClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public ProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductCatalogItem?> GetByIdAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"api/v1/products/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        try
        {
            return await JsonSerializer.DeserializeAsync<ProductCatalogItem>(
                stream,
                JsonOptions,
                cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Product API returned a response that Cart API could not understand.",
                ex);
        }
    }
}
